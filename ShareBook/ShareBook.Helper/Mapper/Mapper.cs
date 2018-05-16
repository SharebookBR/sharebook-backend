using AutoMapper;
using System;

namespace ShareBook.Helper.Mapper
{
    public static class Mapper
    {
        public static TDestination Map<TDestination>(object source)
        {
            return AutoMapper.Mapper.Map<TDestination>(source);
        }

        public static TDestination Map<TSource, TDestination>(TSource source)
        {
            return AutoMapper.Mapper.Map<TSource, TDestination>(source);
        }

        public static TDestination Map<TSource, TDestination>(TSource source, TDestination destination)
        {
            return AutoMapper.Mapper.Map<TSource, TDestination>(source, destination);
        }

        public static void Initialize(Action<IMapperConfigurationExpression> action)
        {
            AutoMapper.Mapper.Initialize(action);
        }
    }
}
