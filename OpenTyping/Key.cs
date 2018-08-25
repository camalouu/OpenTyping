﻿namespace OpenTyping
{
    public class Key
    {
        public string KeyData { get; set; }
        public string ShiftKeyData { get; set; }

        public Key() {}

        public Key(string keyData, string shiftKeyData = null)
        {
            KeyData = keyData;
            ShiftKeyData = shiftKeyData;
        }
    }
}