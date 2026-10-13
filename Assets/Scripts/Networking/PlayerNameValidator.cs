namespace PlayerHostGame.Networking
{
    public static class PlayerNameValidator
    {
        public static bool IsValid(string name)
        {
            if (string.IsNullOrWhiteSpace(name) || name.Length > NetworkConfig.MaxPlayerNameLength)
            {
                return false;
            }

            foreach (char character in name)
            {
                if (char.IsControl(character) || character == NetworkMessage.FieldSeparator)
                {
                    return false;
                }
            }

            return true;
        }
    }
}
