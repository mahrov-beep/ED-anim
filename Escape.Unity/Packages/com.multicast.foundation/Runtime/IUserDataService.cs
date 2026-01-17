namespace Multicast {
    using UserData;

    public interface IUserDataService {
        UdRoot Root { get; }

        void SaveUserData();
    }
}