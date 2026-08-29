using ShareBook.Domain;
using ShareBook.Domain.Enums;
using ShareBook.Helper.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace ShareBook.Service
{
    public static class BookRecommendationRanker
    {
        private static readonly HashSet<string> StopWords = new HashSet<string>(StringComparer.Ordinal)
        {
            "a", "ao", "aos", "as", "com", "como", "da", "das", "de", "do", "dos",
            "e", "ela", "ele", "em", "entre", "era", "essa", "esse", "esta", "este",
            "foi", "mais", "mas", "na", "nas", "no", "nos", "o", "os", "ou", "para",
            "pela", "pelas", "pelo", "pelos", "por", "que", "se", "sem", "seu", "sua",
            "um", "uma", "uns", "umas"
        };

        public static IReadOnlyList<Book> Rank(Book source, IEnumerable<Book> candidates, int limit = 6)
        {
            if (source == null || candidates == null || limit <= 0)
                return Array.Empty<Book>();

            var candidateList = candidates
                .Where(book => book != null && book.Id != source.Id && book.Status == BookStatus.Available)
                .ToList();

            if (candidateList.Count == 0)
                return Array.Empty<Book>();

            var allBooks = candidateList.Append(source).ToList();
            var rawVectors = allBooks.ToDictionary(book => book.Id, BuildRawVector);
            var documentFrequency = BuildDocumentFrequency(rawVectors.Values);
            var documentCount = allBooks.Count;
            var sourceVector = ApplyInverseDocumentFrequency(rawVectors[source.Id], documentFrequency, documentCount);
            var sourceTitleTokens = Tokenize(source.Title).ToHashSet(StringComparer.Ordinal);

            var ranked = candidateList
                .Select(book => new RankedBook(
                    book,
                    Score(source, book, sourceVector,
                        ApplyInverseDocumentFrequency(rawVectors[book.Id], documentFrequency, documentCount),
                        sourceTitleTokens)))
                .OrderByDescending(item => item.Score)
                .ThenByDescending(item => item.Book.CreationDate)
                .ThenBy(item => item.Book.Id)
                .ToList();

            return Diversify(ranked, Math.Min(limit, candidateList.Count));
        }

        private static double Score(
            Book source,
            Book candidate,
            IReadOnlyDictionary<string, double> sourceVector,
            IReadOnlyDictionary<string, double> candidateVector,
            HashSet<string> sourceTitleTokens)
        {
            if (IsEquivalentWork(source, candidate))
                return 1000d + CosineSimilarity(sourceVector, candidateVector);

            var score = CosineSimilarity(sourceVector, candidateVector) * 100d;

            if (SameText(source.Author, candidate.Author))
                score += 8d;

            if (source.CategoryId != Guid.Empty && source.CategoryId == candidate.CategoryId)
                score += 12d;

            var sourceParentId = source.Category?.ParentCategoryId;
            var candidateParentId = candidate.Category?.ParentCategoryId;
            if (sourceParentId.HasValue && sourceParentId == candidateParentId)
                score += 4d;

            var candidateTitleTokens = Tokenize(candidate.Title).ToHashSet(StringComparer.Ordinal);
            score += Containment(sourceTitleTokens, candidateTitleTokens) * 30d;

            if (source.Type == BookType.Printed && candidate.Type == BookType.Eletronic)
                score += 2d;

            return score;
        }

        private static IReadOnlyList<Book> Diversify(IReadOnlyList<RankedBook> ranked, int limit)
        {
            var selected = new List<Book>(limit);
            var deferred = new List<Book>();
            var authorCounts = new Dictionary<string, int>(StringComparer.Ordinal);
            var categoryCounts = new Dictionary<Guid, int>();

            foreach (var item in ranked)
            {
                var book = item.Book;
                var authorKey = Normalize(book.Author);
                authorCounts.TryGetValue(authorKey, out var authorCount);
                categoryCounts.TryGetValue(book.CategoryId, out var categoryCount);

                var duplicatesSelectedWork = selected.Any(existing => IsEquivalentWork(existing, book));
                var exceedsDiversityCap = selected.Count > 0 && (authorCount >= 2 || categoryCount >= 3);

                if (duplicatesSelectedWork || exceedsDiversityCap)
                {
                    deferred.Add(book);
                    continue;
                }

                Add(book, selected, authorCounts, categoryCounts);
                if (selected.Count == limit)
                    return selected;
            }

            foreach (var book in deferred.Where(book => !selected.Any(existing => IsEquivalentWork(existing, book))))
            {
                Add(book, selected, authorCounts, categoryCounts);
                if (selected.Count == limit)
                    break;
            }

            return selected;
        }

        private static void Add(
            Book book,
            ICollection<Book> selected,
            IDictionary<string, int> authorCounts,
            IDictionary<Guid, int> categoryCounts)
        {
            selected.Add(book);

            var authorKey = Normalize(book.Author);
            authorCounts[authorKey] = authorCounts.TryGetValue(authorKey, out var authorCount)
                ? authorCount + 1
                : 1;
            categoryCounts[book.CategoryId] = categoryCounts.TryGetValue(book.CategoryId, out var categoryCount)
                ? categoryCount + 1
                : 1;
        }

        private static bool IsEquivalentWork(Book first, Book second)
        {
            if (!SameText(first.Author, second.Author))
                return false;

            var firstTitle = Tokenize(first.Title).ToHashSet(StringComparer.Ordinal);
            var secondTitle = Tokenize(second.Title).ToHashSet(StringComparer.Ordinal);
            return Containment(firstTitle, secondTitle) >= 0.75d;
        }

        private static Dictionary<string, double> BuildRawVector(Book book)
        {
            var vector = new Dictionary<string, double>(StringComparer.Ordinal);
            AddTokens(vector, book.Title, 5d);
            AddTokens(vector, book.Author, 1.25d);
            AddTokens(vector, book.Category?.Name, 2.5d);
            AddTokens(vector, book.Category?.ParentCategory?.Name, 1.25d);
            AddTokens(vector, book.Synopsis, 1d);
            return vector;
        }

        private static Dictionary<string, int> BuildDocumentFrequency(IEnumerable<Dictionary<string, double>> vectors)
        {
            var frequencies = new Dictionary<string, int>(StringComparer.Ordinal);
            foreach (var vector in vectors)
            {
                foreach (var token in vector.Keys)
                    frequencies[token] = frequencies.TryGetValue(token, out var count) ? count + 1 : 1;
            }

            return frequencies;
        }

        private static Dictionary<string, double> ApplyInverseDocumentFrequency(
            IReadOnlyDictionary<string, double> vector,
            IReadOnlyDictionary<string, int> documentFrequency,
            int documentCount)
        {
            return vector.ToDictionary(
                pair => pair.Key,
                pair => pair.Value * (Math.Log((documentCount + 1d) / (documentFrequency[pair.Key] + 1d)) + 1d),
                StringComparer.Ordinal);
        }

        private static double CosineSimilarity(
            IReadOnlyDictionary<string, double> first,
            IReadOnlyDictionary<string, double> second)
        {
            var dotProduct = first.Sum(pair => pair.Value * (second.TryGetValue(pair.Key, out var value) ? value : 0d));
            var firstMagnitude = Math.Sqrt(first.Values.Sum(value => value * value));
            var secondMagnitude = Math.Sqrt(second.Values.Sum(value => value * value));

            return firstMagnitude == 0d || secondMagnitude == 0d
                ? 0d
                : dotProduct / (firstMagnitude * secondMagnitude);
        }

        private static double Containment(IReadOnlyCollection<string> first, IReadOnlyCollection<string> second)
        {
            if (first.Count == 0 || second.Count == 0)
                return 0d;

            return first.Intersect(second, StringComparer.Ordinal).Count() / (double)Math.Min(first.Count, second.Count);
        }

        private static void AddTokens(IDictionary<string, double> vector, string text, double weight)
        {
            foreach (var token in Tokenize(text))
                vector[token] = vector.TryGetValue(token, out var current) ? current + weight : weight;
        }

        private static IEnumerable<string> Tokenize(string text)
        {
            var normalized = Normalize(text);
            return Regex.Matches(normalized, @"[a-z0-9]+")
                .Select(match => NormalizePlural(match.Value))
                .Where(token => token.Length > 1 && !StopWords.Contains(token));
        }

        private static string NormalizePlural(string token)
            => token.Length > 4 && token.EndsWith("s", StringComparison.Ordinal)
                ? token.Substring(0, token.Length - 1)
                : token;

        private static bool SameText(string first, string second)
            => !string.IsNullOrWhiteSpace(first)
                && string.Equals(Normalize(first), Normalize(second), StringComparison.Ordinal);

        private static string Normalize(string text)
            => (text ?? string.Empty).RemoveAccent().ToLowerInvariant().Trim();

        private sealed class RankedBook
        {
            public RankedBook(Book book, double score)
            {
                Book = book;
                Score = score;
            }

            public Book Book { get; }
            public double Score { get; }
        }
    }
}
