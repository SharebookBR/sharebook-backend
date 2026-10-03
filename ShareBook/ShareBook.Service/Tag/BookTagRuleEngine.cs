using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace ShareBook.Service;

/// <summary>
/// Motor determinístico (sem IA) que sugere tags técnicas a partir do título e da sinopse.
///
/// Portado dos scripts de backfill do sharebook-agent (backfill_technical_tags.py e
/// complete_technical_tags.py), com as mesmas regras de alta confiança já validadas no
/// catálogo técnico. A correspondência só dispara quando o padrão aparece no TÍTULO
/// (a sinopse apenas reforça o score), mantendo a atribuição automática conservadora.
/// </summary>
public static class BookTagRuleEngine
{
    public const int MaxTagsPerBook = 3;

    private const int TitleScore = 3;
    private const int SynopsisScore = 1;
    private const int MinimumScore = 2;

    private sealed record Rule(string TagId, string[] Patterns);

    private static readonly Rule[] Rules = BuildRules();

    /// <summary>
    /// Normaliza texto para comparação: expande C++/C#/.NET, remove acentos,
    /// minúsculas e reduz tudo que não é alfanumérico a espaço único.
    /// </summary>
    public static string NormalizeText(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return string.Empty;
        }

        var text = Regex.Replace(value, @"c\+\+", "cplusplus", RegexOptions.IgnoreCase);
        text = Regex.Replace(text, @"c#", "csharp", RegexOptions.IgnoreCase);
        text = Regex.Replace(text, @"\.net", "dotnet", RegexOptions.IgnoreCase);

        text = text.Normalize(NormalizationForm.FormD);
        var builder = new StringBuilder(text.Length);
        foreach (var ch in text)
        {
            if (CharUnicodeInfo.GetUnicodeCategory(ch) != UnicodeCategory.NonSpacingMark)
            {
                builder.Append(ch);
            }
        }
        text = builder.ToString().Normalize(NormalizationForm.FormC);

        text = text.ToLowerInvariant();
        text = Regex.Replace(text, "[^a-z0-9]+", " ");
        return text.Trim();
    }

    /// <summary>
    /// Retorna até <see cref="MaxTagsPerBook"/> ids de tag ordenados por relevância
    /// (título casou vale mais que só sinopse; empate resolve pela ordem da regra).
    /// </summary>
    public static IReadOnlyList<string> SuggestTagIds(string? title, string? synopsis)
    {
        var titleText = NormalizeText(title);
        var synopsisText = NormalizeText(synopsis);

        var scored = new List<(string TagId, int Score, int Index)>();
        for (var i = 0; i < Rules.Length; i++)
        {
            var rule = Rules[i];
            var score = Score(rule, titleText, synopsisText);
            if (score >= MinimumScore)
            {
                scored.Add((rule.TagId, score, i));
            }
        }

        return scored
            .OrderByDescending(x => x.Score)
            .ThenBy(x => x.Index)
            .Select(x => x.TagId)
            .Distinct(StringComparer.Ordinal)
            .Take(MaxTagsPerBook)
            .ToList();
    }

    private static int Score(Rule rule, string title, string synopsis)
    {
        var score = 0;
        if (rule.Patterns.Any(pattern => Regex.IsMatch(title, pattern)))
        {
            score += TitleScore;
        }
        if (rule.Patterns.Any(pattern => Regex.IsMatch(synopsis, pattern)))
        {
            score += SynopsisScore;
        }
        return score;
    }

    private static Rule[] BuildRules() => new[]
    {
        // ---- Stack / plataforma / frameworks ----
        new Rule("kubernetes", new[] { @"\bkubernetes\b", @"\bk8s\b" }),
        new Rule("docker", new[] { @"\bdocker\b", @"\bcontainers?\b", @"\bconteineres?\b" }),
        new Rule("devops", new[] { @"\bdevops\b", @"\bci cd\b", @"\bpipeline\b", @"\bdeployment\b" }),
        new Rule("aws", new[] { @"\baws\b", @"\bamazon web services\b", @"\bamazon s3\b" }),
        new Rule("cloud", new[] { @"\bcloud\b", @"\bcomputacao em nuvem\b", @"\bcloud computing\b" }),
        new Rule("linux", new[] { @"\blinux\b", @"\bunix\b", @"\bshell\b", @"\bcommand line\b" }),
        new Rule("git", new[] { @"\bgit\b", @"\bgithub\b" }),
        new Rule("networking", new[] { @"\bnetworking\b", @"\btcp ip\b", @"\bipv6\b", @"\bredes de computadores\b", @"\bsockets?\b", @"\bprogramacao em rede\b" }),
        new Rule("observabilidade", new[] { @"\bobservability\b", @"\bobservabilidade\b", @"\bmonitoring\b", @"\blogging\b", @"\bmetrics\b", @"\bdistributed tracing\b" }),
        new Rule("seguranca", new[] { @"\bsecurity\b", @"\bseguranca\b", @"\bhacking\b", @"\bhardening\b", @"\bappsec\b", @"\bselinux\b" }),
        new Rule("criptografia", new[] { @"\bcryptography\b", @"\bcriptografia\b", @"\bcrypto\b" }),
        new Rule("python", new[] { @"\bpython\b" }),
        new Rule("r", new[] { @"\blanguage r\b", @"\blinguagem r\b", @"\busing r\b", @"\busando o r\b", @"\bcom r\b", @"\br para cientistas sociais\b" }),
        new Rule("java", new[] { @"\bjava\b" }),
        new Rule("javascript", new[] { @"\bjavascript\b", @"\bnode js\b", @"\bnodejs\b" }),
        new Rule("typescript", new[] { @"\btypescript\b", @"\btype script\b" }),
        new Rule("csharp", new[] { @"\bcsharp\b", @"\bc sharp\b" }),
        new Rule("dotnet", new[] { @"\bdotnet\b", @"\basp net\b" }),
        new Rule("c-plus-plus", new[] { @"\bcplusplus\b", @"\bcpp\b", @"\bc plus plus\b" }),
        new Rule("c", new[] { @"\blinguagem c\b", @"\bapostila linguagem c\b", @"\bpointers and memory\b" }),
        new Rule("go", new[] { @"\bgolang\b", @"\bgo programming\b", @"\blanguage go\b", @"\bgo por exemplo\b", @"\bgo lang\b", @"\baprenda go\b" }),
        new Rule("php", new[] { @"\bphp\b" }),
        new Rule("scala", new[] { @"\bscala\b" }),
        new Rule("spring-boot", new[] { @"\bspring boot\b", @"\bspring framework\b" }),
        new Rule("sql", new[] { @"\bsql\b" }),
        new Rule("bash", new[] { @"\bbash\b", @"\bshell\b" }),
        new Rule("lisp", new[] { @"\bcommon lisp\b", @"\blisp\b" }),
        new Rule("latex", new[] { @"\blatex\b", @"\blatex2e\b", @"\bbeamer\b" }),
        new Rule("pascal", new[] { @"\bpascal\b" }),
        new Rule("julia", new[] { @"\bjulia\b" }),
        new Rule("fortran", new[] { @"\bfortran\b", @"\bfortran90\b" }),
        new Rule("assembly", new[] { @"\bassembly\b", @"\b8086\b" }),
        new Rule("small-basic", new[] { @"\bsmall basic\b" }),
        new Rule("tkinter", new[] { @"\btkinter\b" }),
        new Rule("yii", new[] { @"\byii\b", @"\byii2\b" }),

        // ---- Dados / IA ----
        new Rule("bancos-de-dados", new[] { @"\bdatabase\b", @"\bdatabases\b", @"\bbanco de dados\b", @"\bbancos de dados\b" }),
        new Rule("data-science", new[] { @"\bdata science\b", @"\bciencia de dados\b", @"\bdata mining\b", @"\bmassive datasets\b", @"\bdata analysis\b" }),
        new Rule("machine-learning", new[] { @"\bmachine learning\b", @"\baprendizado de maquina\b", @"\bstatistical learning\b" }),
        new Rule("deep-learning", new[] { @"\bdeep learning\b", @"\bneural networks?\b", @"\bredes neurais\b" }),
        new Rule("inteligencia-artificial", new[] { @"\bartificial intelligence\b", @"\binteligencia artificial\b", @"\bfoundations of computational agents\b" }),
        new Rule("ia-generativa", new[] { @"\bgenerative ai\b", @"\bchatgpt\b", @"\bdall e\b" }),
        new Rule("prompt-engineering", new[] { @"\bprompt engineering\b", @"\bprompt book\b", @"\bprompts?\b" }),
        new Rule("processamento-de-linguagem-natural", new[] { @"\bnatural language processing\b", @"\bspeech and language\b", @"\bnlp\b", @"\bprocessamento de linguagem natural\b" }),
        new Rule("aprendizado-por-reforco", new[] { @"\breinforcement learning\b", @"\baprendizado por reforco\b" }),
        new Rule("mlops", new[] { @"\bmlops\b", @"\bmachine learning operations\b" }),
        new Rule("computer-vision", new[] { @"\bcomputer vision\b", @"\bvisao computacional\b", @"\bimage processing\b" }),
        new Rule("estatistica", new[] { @"\bstatistics\b", @"\bestatistica\b", @"\bprobability\b", @"\bprobabilidade\b", @"\bbayesian\b", @"\bprobabilistic\b" }),
        new Rule("information-retrieval", new[] { @"\binformation retrieval\b", @"\bsearch engines\b", @"\brecuperacao de informacao\b" }),
        new Rule("otimizacao", new[] { @"\boptimization\b", @"\botimizacao\b", @"\bgenetic programming\b" }),
        new Rule("processamento-de-sinais", new[] { @"\bsignal computing\b", @"\bdigital signals\b", @"\bdsp\b" }),
        new Rule("sistemas-complexos", new[] { @"\bcomplex systems?\b", @"\bcomplexity\b" }),
        new Rule("modelagem", new[] { @"\bmodeling\b", @"\bmodelagem\b" }),

        // ---- Fundamentos / teoria ----
        new Rule("algoritmos", new[] { @"\balgorithms?\b", @"\balgoritmos?\b", @"\balgorithmic\b", @"\bbinary trees?\b", @"\blinked list\b", @"\blist recursion\b", @"\bcompetitive programming\b", @"\bcompetitive programmer\b" }),
        new Rule("estruturas-de-dados", new[] { @"\bdata structures?\b", @"\bestruturas de dados\b", @"\blinked list\b", @"\bbinary trees?\b" }),
        new Rule("sistemas-operacionais", new[] { @"\boperating systems?\b", @"\bsistemas operacionais\b", @"\bkernel\b", @"\bfile system\b", @"\banykernel\b", @"\brump kernels?\b" }),
        new Rule("arquitetura-de-computadores", new[] { @"\bcomputer architecture\b", @"\barquitetura de computadores\b", @"\borganizacao de computadores\b", @"\bcomputer organization\b", @"\bbottom up\b" }),
        new Rule("computacao-de-alto-desempenho", new[] { @"\bhpc\b", @"\bhigh performance computing\b", @"\bcomputacao de alto desempenho\b" }),
        new Rule("metodos-numericos", new[] { @"\bnumerical methods\b", @"\bmetodos numericos\b", @"\banalise numerica\b" }),
        new Rule("compiladores", new[] { @"\bcompilers?\b", @"\bcompiladores\b", @"\bcompiler design\b", @"\bgcc\b", @"\boberon\b" }),
        new Rule("matematica-discreta", new[] { @"\bdiscrete mathematics\b", @"\bmatematica discreta\b" }),
        new Rule("teoria-da-computacao", new[] { @"\btheory of computation\b", @"\bteoria da computacao\b", @"\bautomata\b", @"\bautomatos\b", @"\btheoretical computer science\b", @"\bformal languages?\b" }),
        new Rule("computacao-quantica", new[] { @"\bquantum computing\b", @"\bquantum information\b", @"\bquantum shannon\b" }),
        new Rule("fundamentos-da-computacao", new[] { @"\bintroduction to computer science\b", @"\bfoundations of computer science\b", @"\bcomputer science\b", @"\bfoundations of programming\b" }),
        new Rule("logica", new[] { @"\bcomputational logic\b", @"\blogica\b", @"\blogic\b" }),
        new Rule("pensamento-computacional", new[] { @"\bcomputational thinking\b" }),
        new Rule("teoria-das-categorias", new[] { @"\bcategory theory\b" }),
        new Rule("circuitos-digitais", new[] { @"\bdigital circuits?\b" }),
        new Rule("concorrencia", new[] { @"\bcommunicating sequential processes\b", @"\bcsp\b", @"\bsemaphores?\b", @"\bconcorrencia\b", @"\bconcurrency\b" }),
        new Rule("metodos-formais", new[] { @"\bz specification\b", @"\bformal methods?\b", @"\bsemantics\b", @"\brefinement\b", @"\bproof\b" }),
        new Rule("programacao-funcional", new[] { @"\bfunctional languages?\b", @"\bfunctional programming\b", @"\bprogramacao funcional\b", @"\bcategory theory\b", @"\bstructure and interpretation of computer programs\b", @"\bsicp\b" }),
        new Rule("programacao-orientada-a-objetos", new[] { @"\bobject oriented\b", @"\boop\b", @"\bnaked objects\b" }),

        // ---- Engenharia / arquitetura / processos ----
        new Rule("engenharia-de-software", new[] { @"\bsoftware engineering\b", @"\bengenharia de software\b" }),
        new Rule("microsservicos", new[] { @"\bmicroservices\b", @"\bmicrosservicos\b" }),
        new Rule("arquitetura", new[] { @"\bsoftware architecture\b", @"\barquitetura de software\b", @"\barquitetura\b" }),
        new Rule("apis", new[] { @"\bapi\b", @"\bapis\b", @"\brest\b", @"\bgraphql\b" }),
        new Rule("backend", new[] { @"\bbackend\b", @"\bback end\b", @"\bserver side\b" }),
        new Rule("frontend", new[] { @"\bfrontend\b", @"\bfront end\b", @"\bclient side\b" }),
        new Rule("clean-code", new[] { @"\bclean code\b", @"\bcodigo limpo\b", @"\bcode quality\b", @"\bcode simplicity\b", @"\bsimplicity\b" }),
        new Rule("design-patterns", new[] { @"\bdesign patterns?\b", @"\bpadroes de projeto\b" }),
        new Rule("event-driven", new[] { @"\bevent driven\b", @"\bevent sourcing\b", @"\bstream processing\b", @"\bpub sub\b", @"\bkafka\b", @"\bevent streaming\b" }),
        new Rule("sistemas-distribuidos", new[] { @"\bdistributed systems\b", @"\bsistemas distribuidos\b" }),
        new Rule("testes", new[] { @"\btesting\b", @"\bunit testing\b", @"\btestes automatizados\b", @"\btdd\b" }),
        new Rule("gestao-de-tecnologia", new[] { @"\bit manager\b", @"\bmanagement\b", @"\bdon t just roll the dice\b", @"\bnetworked economy\b" }),
        new Rule("carreira-dev", new[] { @"\bstand out as a software engineer\b", @"\bsoftware engineer\b" }),
        new Rule("escrita-tecnica", new[] { @"\btechnical writing\b", @"\bguidebook\b" }),
        new Rule("software-livre", new[] { @"\bpublic domain\b", @"\bcreative commons\b", @"\bopen source\b", @"\bfree software\b", @"\brichard stallman\b" }),
        new Rule("versionamento-de-codigo", new[] { @"\bsubversion\b", @"\bversion control\b", @"\bsvn\b" }),

        // ---- Sistemas / hardware / outros domínios ----
        new Rule("sistemas-embarcados", new[] { @"\bembedded systems?\b", @"\barduino\b", @"\bdigital circuit\b" }),
        new Rule("internet-das-coisas", new[] { @"\binternet das coisas\b", @"\biot\b" }),
        new Rule("blockchain", new[] { @"\bblockchain\b" }),
        new Rule("criptomoedas", new[] { @"\bbitcoin\b", @"\bcryptocurrency\b", @"\bcryptocurrencies\b" }),
        new Rule("processamento-de-imagens", new[] { @"\bjpeg\b", @"\bimage processing\b", @"\bprocessamento de imagens\b" }),
        new Rule("realidade-virtual", new[] { @"\bvirtual reality\b", @"\bvirtual worlds?\b", @"\brealidade virtual\b" }),
        new Rule("vim", new[] { @"\bvim\b", @"\bneovim\b", @"\bvi improved\b", @"\bgvim\b" }),
        new Rule("editores-de-texto", new[] { @"\bemacs\b", @"\btext editing\b", @"\beditor de texto\b" }),
        new Rule("computacao-grafica", new[] { @"\bcomputer graphics\b", @"\bcomputacao grafica\b", @"\bopengl\b", @"\bray tracing\b", @"\brendering\b", @"\b3d graphics\b" }),
        new Rule("desenvolvimento-de-jogos", new[] { @"\bgame development\b", @"\bgame dev\b", @"\bdesenvolvimento de jogos\b", @"\bpygame\b", @"\bgames\b", @"\bgame programming\b" }),
        new Rule("html-css", new[] { @"\bhtml\b", @"\bcss\b" }),
        new Rule("web-design", new[] { @"\bweb design\b", @"\bdesign web\b" }),
    };
}
