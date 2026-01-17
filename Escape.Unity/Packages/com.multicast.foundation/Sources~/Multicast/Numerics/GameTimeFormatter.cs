using MessagePack;
using Multicast.Numerics;

[assembly: MessagePackKnownFormatter(typeof(GameTimeFormatter))]

namespace Multicast.Numerics {
    using MessagePack;
    using MessagePack.Formatters;

    public class GameTimeFormatter : IMessagePackFormatter<GameTime> {
        public static readonly GameTimeFormatter Instance = new GameTimeFormatter();

        private GameTimeFormatter() {
        }

        public void Serialize(ref MessagePackWriter writer, GameTime value, MessagePackSerializerOptions options) {
            writer.Write(value.AsDateTime);
        }

        public GameTime Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options) {
            return new GameTime {
                ticks = reader.ReadDateTime().Ticks,
            };
        }
    }
}