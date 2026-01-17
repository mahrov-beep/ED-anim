namespace Game.ServerRunner.Db;

public static class PostgressHelper {
    /// <summary>
    /// Compares two Guid values using PostgreSQL's uuid sort order.
    /// </summary>
    /// <remarks>
    /// Converts the .NET Guid byte layout to RFC UUID network order (big-endian for time_low/time_mid/time_hi_and_version),
    /// then performs a lexicographic 16-byte comparison, which matches PostgreSQL's internal comparator for type uuid.
    /// References:
    /// • RFC 9562 (current UUID spec): https://www.rfc-editor.org/rfc/rfc9562
    /// • RFC 4122 (historical UUID spec): https://www.rfc-editor.org/rfc/rfc4122
    /// • PostgreSQL 16 — UUID type (links to RFC 4122): https://www.postgresql.org/docs/16/datatype-uuid.html
    /// • PostgreSQL 16 — UUID functions: https://www.postgresql.org/docs/16/functions-uuid.html
    /// • PostgreSQL source — uuid_internal_cmp uses memcmp over 16 bytes: https://doxygen.postgresql.org/uuid_8c.html
    /// </remarks>
    /// <param name="x">First Guid.</param>
    /// <param name="y">Second Guid.</param>
    /// <returns>-1 if x &lt; y, 0 if equal, 1 if x &gt; y.</returns>
    public static int CompareGuidAsPostgres(Guid x, Guid y) {
        var xb = x.ToByteArray();
        var yb = y.ToByteArray();

        Span<byte> xa = stackalloc byte[16];
        Span<byte> ya = stackalloc byte[16];

        xa[0] = xb[3];
        xa[1] = xb[2];
        xa[2] = xb[1];
        xa[3] = xb[0];
        xa[4] = xb[5];
        xa[5] = xb[4];
        xa[6] = xb[7];
        xa[7] = xb[6];
        xa[8] = xb[8];
        xa[9] = xb[9];
        xa[10] = xb[10];
        xa[11] = xb[11];
        xa[12] = xb[12];
        xa[13] = xb[13];
        xa[14] = xb[14];
        xa[15] = xb[15];

        ya[0] = yb[3];
        ya[1] = yb[2];
        ya[2] = yb[1];
        ya[3] = yb[0];
        ya[4] = yb[5];
        ya[5] = yb[4];
        ya[6] = yb[7];
        ya[7] = yb[6];
        ya[8] = yb[8];
        ya[9] = yb[9];
        ya[10] = yb[10];
        ya[11] = yb[11];
        ya[12] = yb[12];
        ya[13] = yb[13];
        ya[14] = yb[14];
        ya[15] = yb[15];

        for (int i = 0; i < 16; i++) {
            if (xa[i] == ya[i]) {
                continue;
            }
            return xa[i] < ya[i] ? -1 : 1;
        }
        return 0;
    }
}


