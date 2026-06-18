using System.Linq;
using RAT_Logic;
using Xunit;

namespace RAT_Tests
{
    //KI start (Claude Opus 4.8, prompt 21): 10 xUnit tests for the RAT_Logic layer.
    // Covers IP validation, the abstract DeviceDescriptor hierarchy, and the per-user access-rights rules.
    public class RatLogicTests
    {
        // helper: a normal (non-admin) user
        private static NetworkUser User(int id, int privileges = 10) =>
            new NetworkUser($"user{id}", id, canCreate: false, privileges: privileges);

        // 1) IP validation accepts a well-formed IPv4 address
        [Fact]
        public void IsIpv4Valid_AcceptsValidAddress()
        {
            Assert.True(IP.IsIpv4Valid("192.168.1.1"));
        }

        // 2) IP validation rejects an out-of-range / malformed address
        [Fact]
        public void IsIpv4Valid_RejectsInvalidAddress()
        {
            Assert.False(IP.IsIpv4Valid("999.1.1.1"));
            Assert.False(IP.IsIpv4Valid("not-an-ip"));
        }

        // 3) Port validation: 0..65535 ok, above is not
        [Fact]
        public void IsPortValid_ChecksRange()
        {
            Assert.True(IP.IsPortVlaid(22));
            Assert.True(IP.IsPortVlaid(65535));
            Assert.False(IP.IsPortVlaid(70000));
        }

        // 4) Abstract DeviceDescriptor factory returns the matching concrete subclass per type
        [Fact]
        public void DeviceDescriptor_For_ReturnsMatchingConcreteType()
        {
            Assert.IsType<PcDescriptor>(DeviceDescriptor.For(NetworkObjectType.PC));
            Assert.IsType<RouterDescriptor>(DeviceDescriptor.For(NetworkObjectType.Router));
            Assert.IsType<ServerDescriptor>(DeviceDescriptor.For(NetworkObjectType.Server));
        }

        // 5) Polymorphism: only the PC descriptor can use the real host's specs
        [Fact]
        public void DeviceDescriptor_OnlyPcCanUseHostSpecs()
        {
            Assert.True(DeviceDescriptor.For(NetworkObjectType.PC).CanUseHostSpecs);
            Assert.False(DeviceDescriptor.For(NetworkObjectType.Router).CanUseHostSpecs);
            Assert.False(DeviceDescriptor.For(NetworkObjectType.Switch).CanUseHostSpecs);
        }

        // 6) NetworkObject.Descriptor reflects its Type and carries the right icon key
        [Fact]
        public void NetworkObject_Descriptor_MatchesTypeAndIcon()
        {
            NetworkObject pc = new NetworkObject { Type = NetworkObjectType.PC };
            Assert.Equal(NetworkObjectType.PC, pc.Descriptor.Type);
            Assert.Equal("Icon.PC", pc.Descriptor.IconKey);
        }

        // 7) A user with no permission row is Hidden (0) by default
        [Fact]
        public void GetRight_DefaultsToHidden()
        {
            NetworkObject obj = new NetworkObject { Type = NetworkObjectType.Router };
            Assert.Equal(AccesRights.Hidden, obj.GetRight(User(1)));
        }

        // 8) A global admin (privileges >= 100) implicitly owns every object
        [Fact]
        public void GetRight_GlobalAdminIsOwnerEverywhere()
        {
            NetworkObject obj = new NetworkObject { Type = NetworkObjectType.Router };
            NetworkUser admin = User(1, privileges: 100);
            Assert.Equal(AccesRights.Owner, obj.GetRight(admin));
            Assert.True(obj.CanBeDeletedBy(admin));
        }

        // 9) Owner may grant Admin/Owner; an Admin may not (and can't touch other admins)
        [Fact]
        public void CanChangeRight_EnforcesAdminVsOwnerRules()
        {
            NetworkObject obj = new NetworkObject { Type = NetworkObjectType.Server };
            NetworkUser owner = User(1);
            NetworkUser admin = User(2);
            NetworkUser member = User(3);
            obj.ApplyRight(owner, AccesRights.Owner);
            obj.ApplyRight(admin, AccesRights.Admin);
            obj.ApplyRight(member, AccesRights.See);

            // owner can promote a member to Admin
            Assert.True(obj.CanChangeRight(owner, member, AccesRights.Admin));
            // admin can set a lower user to Edit...
            Assert.True(obj.CanChangeRight(admin, member, AccesRights.Edit));
            // ...but may NOT grant Admin/Owner...
            Assert.False(obj.CanChangeRight(admin, member, AccesRights.Owner));
            // ...nor change another Admin/Owner
            Assert.False(obj.CanChangeRight(admin, owner, AccesRights.See));
            // nobody may change their own rights here
            Assert.False(obj.CanChangeRight(owner, owner, AccesRights.Edit));
        }

        // 10) SetRight applies when allowed and is a no-op when not; ApplyRight(Hidden) removes the row
        [Fact]
        public void SetRight_AppliesWhenAllowed_AndHiddenRemovesEntry()
        {
            NetworkObject obj = new NetworkObject { Type = NetworkObjectType.Client };
            NetworkUser owner = User(1);
            NetworkUser member = User(2);
            obj.ApplyRight(owner, AccesRights.Owner);

            // owner grants See -> succeeds and is reflected
            Assert.True(obj.SetRight(owner, member, AccesRights.See));
            Assert.Equal(AccesRights.See, obj.GetRight(member));

            // member (See) tries to grant themselves Owner -> rejected, unchanged
            Assert.False(obj.SetRight(member, owner, AccesRights.Hidden));
            Assert.Equal(AccesRights.Owner, obj.GetRight(owner));

            // setting Hidden removes the entry entirely
            obj.ApplyRight(member, AccesRights.Hidden);
            Assert.Equal(AccesRights.Hidden, obj.GetRight(member));
            Assert.DoesNotContain(obj.AccessRights, a => a.User.ID == member.ID);
        }
    }
    //KI end
}
