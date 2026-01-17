namespace Tests.E2E.Friends;

using Game.Shared.DTO;
using Multicast;
using Xunit;
using System.Linq;
[Collection("e2e-host")]
// ReSharper disable once InconsistentNaming
public class Runner(E2EHost host) {

    private async Task<FriendsActor> CreateUserAsync(string deviceId) {
        return await FriendsActor.CreateAsync(host, deviceId);
    }

    [Fact]
    public async Task Friends_Add_PendingCreated() {
        FriendsActor a = await CreateUserAsync("dev-a");
        FriendsActor b = await CreateUserAsync("dev-b");

        ServerResult<FriendAddResponse> result = await a.AddAsync(b.Id);
        result.ShouldBePendingCreated();
    }

    [Fact]
    public async Task Friends_Add_Repeat_AlreadyPendingOutgoing() {
        FriendsActor a = await CreateUserAsync("dev-a2");
        FriendsActor b = await CreateUserAsync("dev-b2");

        _ = await a.AddAsync(b.Id);
        ServerResult<FriendAddResponse> result = await a.AddAsync(b.Id);
        result.ShouldBeAlreadyPendingOutgoing();
    }

    [Fact]
    public async Task Friends_Accept_Then_FriendsAppearInLists() {
        FriendsActor a = await CreateUserAsync("dev-a3");
        FriendsActor b = await CreateUserAsync("dev-b3");

        _ = await a.AddAsync(b.Id);

        ServerResult<FriendAcceptResponse> result = await b.AcceptAsync(a.Id);
        result.ShouldBeAlreadyFriends();

        ServerResult<FriendsListResponse> listA = await a.FriendsAsync();
        listA.ShouldContainFriend(b.Id);

        ServerResult<FriendsListResponse> listB = await b.FriendsAsync();
        listB.ShouldContainFriend(a.Id);
    }

    [Fact]
    public async Task Friends_Add_Self_ControllerError() {
        FriendsActor a = await CreateUserAsync("dev-a4");

        ServerResult<FriendAddResponse> result = await a.AddAsync(a.Id);
        result.AssertDataIsNull();
    }

    [Fact]
    public async Task Friends_Add_EmptyGuid_ControllerError() {
        FriendsActor a = await CreateUserAsync("dev-a5");

        ServerResult<FriendAddResponse> result = await a.AddAsync(Guid.Empty);
        result.AssertDataIsNull();
    }

    [Fact]
    public async Task Friends_Accept_Without_Pending_NotFound() {
        FriendsActor b = await CreateUserAsync("dev-b6");
        FriendsActor c = await CreateUserAsync("dev-c6");

        ServerResult<FriendAcceptResponse> result = await b.AcceptAsync(c.Id);

        result.ShouldBeNotFound();
    }

    [Fact]
    public async Task Friends_Decline_RemovesFromIncoming() {
        FriendsActor a = await CreateUserAsync("dev-a7");
        FriendsActor b = await CreateUserAsync("dev-b7");

        _ = await a.AddAsync(b.Id);

        ServerResult<IncomingRequestsResponse> incomingBefore = await b.IncomingAsync();
        incomingBefore.ShouldContainIncoming(a.Id);

        ServerResult<FriendDeclineResponse> decline = await b.DeclineAsync(a.Id);
        decline.ShouldBeRemoved();

        ServerResult<IncomingRequestsResponse> incomingAfter = await b.IncomingAsync();
        incomingAfter.ShouldNotContainIncoming(a.Id);

        ServerResult<FriendsListResponse> listA = await a.FriendsAsync();
        listA.ShouldNotContainFriend(b.Id);
    }

    [Fact]
    public async Task Friends_Remove_AfterAccept_RemovesFromBothLists() {
        FriendsActor a = await CreateUserAsync("dev-a8");
        FriendsActor b = await CreateUserAsync("dev-b8");

        _ = await a.AddAsync(b.Id);
        _ = await b.AcceptAsync(a.Id);

        ServerResult<FriendsListResponse> beforeA = await a.FriendsAsync();
        ServerResult<FriendsListResponse> beforeB = await b.FriendsAsync();

        beforeA.ShouldContainFriend(b.Id);
        beforeB.ShouldContainFriend(a.Id);

        ServerResult<FriendRemoveResponse> remove = await a.RemoveAsync(b.Id);
        remove.ShouldBeRemoved();

        ServerResult<FriendsListResponse> afterA = await a.FriendsAsync();
        ServerResult<FriendsListResponse> afterB = await b.FriendsAsync();

        afterA.ShouldNotContainFriend(b.Id);
        afterB.ShouldNotContainFriend(a.Id);
    }

    [Fact]
    public async Task Friends_FullFlow() {
        FriendsActor a = await CreateUserAsync("dev-a9");
        FriendsActor b = await CreateUserAsync("dev-b9");

        ServerResult<FriendAddResponse> add1 = await a.AddAsync(b.Id);
        add1.ShouldBePendingCreated();

        await host.ShouldHavePendingFromToAsync(a.Id, b.Id);

        ServerResult<IncomingRequestsResponse> inc1 = await b.IncomingAsync();
        inc1.ShouldContainIncoming(a.Id);

        ServerResult<FriendDeclineResponse> dec = await b.DeclineAsync(a.Id);
        dec.ShouldBeRemoved();

        await host.ShouldNotHaveFriendshipAsync(a.Id, b.Id);
        ServerResult<IncomingRequestsResponse> inc2 = await b.IncomingAsync();

        inc2.ShouldNotContainIncoming(a.Id);

        ServerResult<FriendAddResponse> add2 = await a.AddAsync(b.Id);
        add2.ShouldBePendingCreated();

        await host.ShouldHavePendingFromToAsync(a.Id, b.Id);

        ServerResult<FriendAcceptResponse> acc = await b.AcceptAsync(a.Id);
        acc.ShouldBeAlreadyFriends();

        await host.ShouldHaveAcceptedFriendshipAsync(a.Id, b.Id);

        ServerResult<FriendsListResponse> fl1 = await a.FriendsAsync();
        ServerResult<FriendsListResponse> fl2 = await b.FriendsAsync();

        fl1.ShouldContainFriend(b.Id);
        fl2.ShouldContainFriend(a.Id);

        ServerResult<FriendRemoveResponse> rem = await a.RemoveAsync(b.Id);
        Assert.NotNull(rem);

        await host.ShouldNotHaveFriendshipAsync(a.Id, b.Id);

        ServerResult<FriendsListResponse> fa = await a.FriendsAsync();
        ServerResult<FriendsListResponse> fb = await b.FriendsAsync();

        fa.ShouldNotContainFriend(b.Id);
        fb.ShouldNotContainFriend(a.Id);
    }

    [Fact]
    public async Task Friends_Add_Reverse_AutoAccept_And_DB_Accepted() {
        FriendsActor a = await CreateUserAsync("dev-a10");
        FriendsActor b = await CreateUserAsync("dev-b10");

        ServerResult<FriendAddResponse> addA = await a.AddAsync(b.Id);
        addA.ShouldBePendingCreated();
        await host.ShouldHavePendingFromToAsync(a.Id, b.Id);

        ServerResult<FriendAddResponse> addB = await b.AddAsync(a.Id);
        addB.ShouldBeAlreadyFriendsNotCreated();
        await host.ShouldHaveAcceptedFriendshipAsync(a.Id, b.Id);

        ServerResult<FriendsListResponse> listA = await a.FriendsAsync();
        ServerResult<FriendsListResponse> listB = await b.FriendsAsync();

        listA.ShouldContainFriend(b.Id);
        listB.ShouldContainFriend(a.Id);
    }

    [Fact]
    public async Task Friends_Bulk_AcceptAll() {
        FriendsActor a1 = await CreateUserAsync("dev-a-bulk-1");
        FriendsActor a2 = await CreateUserAsync("dev-a-bulk-2");
        FriendsActor a3 = await CreateUserAsync("dev-a-bulk-3");
        FriendsActor b  = await CreateUserAsync("dev-b-bulk-acc-all");

        _ = await a1.AddAsync(b.Id);
        _ = await a2.AddAsync(b.Id);
        _ = await a3.AddAsync(b.Id);

        ServerResult<FriendsIncomingBulkResponse> bulk = await b.IncomingBulkAsync(
            new FriendsIncomingBulkRequest {
                Action = EFriendBulkAction.Accept,
            });

        await host.ShouldHaveAcceptedFriendshipAsync(a1.Id, b.Id);
        await host.ShouldHaveAcceptedFriendshipAsync(a2.Id, b.Id);
        await host.ShouldHaveAcceptedFriendshipAsync(a3.Id, b.Id);

        (await a1.FriendsAsync()).ShouldContainFriend(b.Id);
        (await a2.FriendsAsync()).ShouldContainFriend(b.Id);
        (await a3.FriendsAsync()).ShouldContainFriend(b.Id);
        (await b.FriendsAsync()).ShouldContainFriend(a1.Id);
        (await b.FriendsAsync()).ShouldContainFriend(a2.Id);
        (await b.FriendsAsync()).ShouldContainFriend(a3.Id);
    }

    [Fact]
    public async Task Friends_Bulk_DeclineAll() {
        FriendsActor a1 = await CreateUserAsync("dev-a-bulk-d-1");
        FriendsActor a2 = await CreateUserAsync("dev-a-bulk-d-2");
        FriendsActor b  = await CreateUserAsync("dev-b-bulk-dec-all");

        _ = await a1.AddAsync(b.Id);
        _ = await a2.AddAsync(b.Id);

        ServerResult<FriendsIncomingBulkResponse> bulk = await b.IncomingBulkAsync(
            new FriendsIncomingBulkRequest {
                Action = EFriendBulkAction.Decline,
            });

        ServerResult<IncomingRequestsResponse> incomingAfter = await b.IncomingAsync();
        incomingAfter.ShouldNotContainIncoming(a1.Id);
        incomingAfter.ShouldNotContainIncoming(a2.Id);

        await host.ShouldNotHaveFriendshipAsync(a1.Id, b.Id);
        await host.ShouldNotHaveFriendshipAsync(a2.Id, b.Id);
    }

    [Fact]
    public async Task Friends_Bulk_AcceptAll_After_OneDeclined_Manually() {
        FriendsActor a1 = await CreateUserAsync("dev-a-bulk-mid-1");
        FriendsActor a2 = await CreateUserAsync("dev-a-bulk-mid-2");
        FriendsActor a3 = await CreateUserAsync("dev-a-bulk-mid-3");
        FriendsActor b  = await CreateUserAsync("dev-b-bulk-mid");

        _ = await a1.AddAsync(b.Id);
        _ = await a2.AddAsync(b.Id);
        _ = await a3.AddAsync(b.Id);

        ServerResult<FriendDeclineResponse> decline = await b.DeclineAsync(a2.Id);
        decline.ShouldBeRemoved();

        ServerResult<FriendsIncomingBulkResponse> bulk = await b.IncomingBulkAsync(
            new FriendsIncomingBulkRequest {
                Action = EFriendBulkAction.Accept,
            });

        await host.ShouldHaveAcceptedFriendshipAsync(a1.Id, b.Id);
        await host.ShouldHaveAcceptedFriendshipAsync(a3.Id, b.Id);
        await host.ShouldNotHaveFriendshipAsync(a2.Id, b.Id);

        (await b.FriendsAsync()).ShouldContainFriend(a1.Id);
        (await b.FriendsAsync()).ShouldContainFriend(a3.Id);
        (await b.FriendsAsync()).ShouldNotContainFriend(a2.Id);

        (await a1.FriendsAsync()).ShouldContainFriend(b.Id);
        (await a3.FriendsAsync()).ShouldContainFriend(b.Id);
        (await a2.FriendsAsync()).ShouldNotContainFriend(b.Id);
    }
}