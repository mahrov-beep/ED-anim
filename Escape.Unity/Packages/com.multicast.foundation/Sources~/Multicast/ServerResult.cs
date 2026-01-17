namespace Multicast {
    using MessagePack;

    public interface IServerRequest {
    }

    public interface IServerResponse {
    }

    [MessagePackObject]
    public class ServerResult<T> where T : class, IServerResponse {
        [Key(0)] public int ErrorCode;
        [Key(1)] public string ErrorMessage;
        [Key(2)] public T Data;

        public static implicit operator ServerResult<T>(ServerResult response) => new ServerResult<T> {
            ErrorCode = response.ErrorCode,
            ErrorMessage = response.ErrorMessage,
        };

        public static implicit operator ServerResult<T>(T data) => new ServerResult<T> {
            Data = data,
        };
    }

    [MessagePackObject]
    public struct ServerResult {
        [Key(0)] public int ErrorCode;
        [Key(1)] public string ErrorMessage;

        public static ServerResult Error(int code, string message) => new ServerResult {
            ErrorCode = code,
            ErrorMessage = message,
        };
    }
}