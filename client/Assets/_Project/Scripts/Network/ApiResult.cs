namespace CatRoyale.Network
{
    public class ApiResult<T>
    {
        public bool Success { get; private set; }
        public T Data { get; private set; }
        public string Error { get; private set; }
        public int StatusCode { get; private set; }

        public static ApiResult<T> Ok(T data, int statusCode = 200)
            => new ApiResult<T> { Success = true, Data = data, StatusCode = statusCode };

        public static ApiResult<T> Fail(string error, int statusCode = 0)
            => new ApiResult<T> { Success = false, Error = error, StatusCode = statusCode };
    }
}