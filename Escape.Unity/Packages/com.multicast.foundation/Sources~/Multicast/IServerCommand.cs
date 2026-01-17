using Multicast.ServerData;
using System;
using System.Threading.Tasks;

namespace Multicast {
    public interface IServerCommand<TServerData>
        where TServerData : SdObject {
    }

    public interface IServerCommandExecutableFromClient {
    }

    public interface IServerCommandClientSidePredictable {
    }

    public interface IServerCommandContext {
    }

    public interface IServerCommandHandlerBase {
    }

    public interface IServerCommandHandler<TServerDataContext, TServerData>
        : IServerCommandHandlerBase
        where TServerDataContext : class, IServerCommandContext
        where TServerData : SdObject {
        Task<ServerCommandResult> Execute(TServerDataContext context, TServerData gameData, IServerCommand<TServerData> command);
    }

    public interface IServerCommandHandler<TServerDataContext, TServerData, TServerCommand>
        : IServerCommandHandler<TServerDataContext, TServerData>
        where TServerDataContext : class, IServerCommandContext
        where TServerData : SdObject
        where TServerCommand : class, IServerCommand<TServerData> {
        Task<ServerCommandResult> Execute(TServerDataContext context, TServerData gameData, TServerCommand command);
    }

    public abstract class ServerCommandHandler<TServerDataContext, TServerData, TServerCommand>
        : IServerCommandHandler<TServerDataContext, TServerData, TServerCommand>
        where TServerDataContext : class, IServerCommandContext
        where TServerData : SdObject
        where TServerCommand : class, IServerCommand<TServerData> {
        public abstract Task<ServerCommandResult> Execute(TServerDataContext context, TServerData gameData, TServerCommand command);

        Task<ServerCommandResult> IServerCommandHandler<TServerDataContext, TServerData>.Execute(TServerDataContext context, TServerData gameData, IServerCommand<TServerData> command) {
            return this.Execute(context, gameData, (TServerCommand)command);
        }

        protected static ServerCommandResult Ok => new ServerCommandResult(ServerCommandResultCode.Ok, null);

        protected static ServerCommandResult BadRequest(string error) {
            throw new ServerCommandResultException(new ServerCommandResult(ServerCommandResultCode.BadRequest, error));
        }

        protected static ServerCommandResult InternalError(string error) {
            throw new ServerCommandResultException(new ServerCommandResult(ServerCommandResultCode.InternalError, error));
        }
    }

    public class ServerCommandResultException : Exception {
        public ServerCommandResultException(ServerCommandResult result) {
            this.Result = result;
        }

        public ServerCommandResult Result { get; }
    }

    public struct ServerCommandResult {
        public ServerCommandResultCode ResultCode;
        public string                  Error;

        public ServerCommandResult(ServerCommandResultCode resultCode, string error) {
            this.ResultCode = resultCode;
            this.Error      = error ?? string.Empty;
        }
    }

    public enum ServerCommandResultCode {
        Ok,
        BadRequest,
        InternalError,
    }
}