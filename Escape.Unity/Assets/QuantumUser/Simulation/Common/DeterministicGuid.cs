namespace Quantum {
  using System;
  using System.Security.Cryptography;
  using System.Text;

  // https://en.wikipedia.org/wiki/Universally_unique_identifier#Versions_3_and_5_(namespace_name-based)
  // https://github.com/Informatievlaanderen/deterministic-guid-generator
  public class DeterministicGuid {
    public static Guid Create(Guid namespaceId, string name) => Create(namespaceId, name, 5);

    static Guid Create(Guid namespaceId, string name, int version) {
      if (namespaceId == Guid.Empty) {
        throw new ArgumentException("Namespace cannot be an empty GUID.", nameof(namespaceId));
      }

      if (namespaceId == Guid.Empty) {
        throw new ArgumentNullException(nameof(namespaceId), "Namespace cannot be null or empty.");
      }

      if (string.IsNullOrEmpty(name)) {
        throw new ArgumentNullException(nameof(name), "Name cannot be null or empty.");
      }

      if (version != 3 && version != 5) {
        throw new ArgumentOutOfRangeException(nameof(version), "version must be either 3 or 5.");
      }

      var nameBytes = Encoding.UTF8.GetBytes(name);

      if (nameBytes.Length == 0) {
        throw new ArgumentNullException(nameof(nameBytes));
      }

      var namespaceBytes = namespaceId.ToByteArray();
      SwapByteOrder(namespaceBytes);

      byte[] hash;
      using (var algorithm = version == 3 ? (HashAlgorithm)MD5.Create() : SHA1.Create()) {
        var combinedBytes = new byte[namespaceBytes.Length + nameBytes.Length];
        Buffer.BlockCopy(namespaceBytes, 0, combinedBytes, 0, namespaceBytes.Length);
        Buffer.BlockCopy(nameBytes, 0, combinedBytes, namespaceBytes.Length, nameBytes.Length);

        hash = algorithm.ComputeHash(combinedBytes);
      }

      var newGuid = new byte[16];
      Array.Copy(hash, 0, newGuid, 0, 16);

      newGuid[6] = (byte)((newGuid[6] & 0x0F) | (version << 4));
      newGuid[8] = (byte)((newGuid[8] & 0x3F) | 0x80);

      SwapByteOrder(newGuid);
      return new Guid(newGuid);
    }

    static void SwapByteOrder(byte[] guid) {
      SwapBytes(guid, 0, 3);
      SwapBytes(guid, 1, 2);
      SwapBytes(guid, 4, 5);
      SwapBytes(guid, 6, 7);
    }

    static void SwapBytes(byte[] guid, int left, int right) {
      (guid[left], guid[right]) = (guid[right], guid[left]);
    }
  }
}