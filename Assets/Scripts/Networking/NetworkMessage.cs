using System;
using System.Globalization;
using System.Text;

namespace PlayerHostGame.Networking
{
    public readonly struct NetworkMessage
    {
        public const char FieldSeparator = '|';

        public MessageType Type { get; }
        public string[] Fields { get; }

        public NetworkMessage(MessageType type, params string[] fields)
        {
            Type = type;
            Fields = fields ?? Array.Empty<string>();
        }

        public bool HasFields(int count)
        {
            return Fields.Length >= count;
        }

        public bool TryGetInt(int index, out int value)
        {
            value = 0;
            return index < Fields.Length
                && int.TryParse(Fields[index], NumberStyles.Integer, CultureInfo.InvariantCulture, out value);
        }

        public bool TryGetLong(int index, out long value)
        {
            value = 0;
            return index < Fields.Length
                && long.TryParse(Fields[index], NumberStyles.Integer, CultureInfo.InvariantCulture, out value);
        }

        public bool TryGetFloat(int index, out float value)
        {
            value = 0f;
            return index < Fields.Length
                && float.TryParse(Fields[index], NumberStyles.Float, CultureInfo.InvariantCulture, out value)
                && !float.IsNaN(value)
                && !float.IsInfinity(value);
        }

        public byte[] ToBytes()
        {
            string[] parts = new string[Fields.Length + 1];
            parts[0] = ((int)Type).ToString(CultureInfo.InvariantCulture);
            Array.Copy(Fields, 0, parts, 1, Fields.Length);
            return Encoding.UTF8.GetBytes(string.Join(FieldSeparator.ToString(), parts));
        }

        public static bool TryParse(byte[] data, out NetworkMessage message)
        {
            message = default;
            string[] parts = Encoding.UTF8.GetString(data).Split(FieldSeparator);

            bool hasValidType = int.TryParse(parts[0], NumberStyles.Integer, CultureInfo.InvariantCulture, out int typeValue)
                && Enum.IsDefined(typeof(MessageType), typeValue);
            if (!hasValidType)
            {
                return false;
            }

            string[] fields = new string[parts.Length - 1];
            Array.Copy(parts, 1, fields, 0, fields.Length);
            message = new NetworkMessage((MessageType)typeValue, fields);
            return true;
        }
    }
}
