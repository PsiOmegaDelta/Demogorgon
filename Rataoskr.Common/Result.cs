namespace Rataoskr.Common
{
    public class Result
    {
        protected Result(bool success, string error)
        {
            this.Success = success;
            this.Error = error;
        }

        public string Error { get; private set; }

        public bool Failure => !this.Success;

        public bool Success { get; }

        public static Result Combine(params Result[] results)
        {
            foreach (var result in results)
            {
                if (result.Failure)
                {
                    return result;
                }
            }

            return Ok();
        }

        public static Result Fail(string message)
        {
            return new Result(false, message);
        }

        public static Result<T> Fail<T>(string message)
        {
            return new Result<T>(default(T), false, message);
        }

        public static Result Ok()
        {
            return new Result(true, string.Empty);
        }

        public static Result<T> Ok<T>(T value)
        {
            return new Result<T>(value, true, string.Empty);
        }
    }

    public class Result<T> : Result
    {
        protected internal Result(T value, bool success, string error)
            : base(success, error)
        {
            this.Value = value;
        }

        public T Value { get; private set; }
    }
}
