using FluentResults;

namespace Dashboard.Transformers; 

public interface IMapper<in T, TT> where T : class where TT : class {
    Result<TT> Map(T source);
    Result<TT> Map(T source, TT destination);
}