using System;
using System.Collections.Generic;
using System.Linq;

namespace NetSdrClientApp.Messages
{
    public static class NetSdrMessageHelper
    {
        // ✅ CORRECT: Enums are INSIDE the class
        public enum MsgTypes : ushort
    {
        Ack = 0,
        Nack = 1,
        GetControlItem = 2,
        SetControlItem = 4,
        DataItem = 5,   // БЫЛО 8 -> СТАЛО 5 (чтобы влезть в 3 бита)
        DataItem2 = 6   // БЫЛО 9 -> СТАЛО 6
    }

        public enum ControlItemCodes : ushort
        {
            ReceiverState = 0x0018,
            ReceiverFrequency = 0x0020,
            IQOutputDataSampleRate = 0x00B0,
            RFFilter = 0x0044,
            ADModes = 0x008A
        }

        // --- Methods ---

        public static byte[] GetControlItemMessage(MsgTypes type, ControlItemCodes code, byte[] parameters)
        {
            ushort length = (ushort)(2 + 2 + parameters.Length); 
            ushort header = (ushort)((ushort)type << 13 | length);

            byte[] message = new byte[length];
            BitConverter.GetBytes(header).CopyTo(message, 0);
            BitConverter.GetBytes((ushort)code).CopyTo(message, 2);
            parameters.CopyTo(message, 4);

            return message;
        }

        public static byte[] GetDataItemMessage(MsgTypes type, byte[] parameters)
        {
             ushort length = (ushort)(2 + parameters.Length);
             ushort header = (ushort)((ushort)type << 13 | length);
             byte[] message = new byte[length];
             BitConverter.GetBytes(header).CopyTo(message, 0);
             parameters.CopyTo(message, 2);
             return message;
        }

        public static void TranslateMessage(byte[] message, out MsgTypes type, out ControlItemCodes code, out ushort sequenceNum, out byte[] body)
        {
            if (message == null || message.Length < 2)
            {
                type = MsgTypes.Nack;
                code = 0;
                sequenceNum = 0;
                body = Array.Empty<byte>();
                return;
            }

            ushort header = BitConverter.ToUInt16(message, 0);
            type = (MsgTypes)(header >> 13);
            
            if (type == MsgTypes.SetControlItem || type == MsgTypes.GetControlItem || type == MsgTypes.Ack || type == MsgTypes.Nack)
            {
                code = (ControlItemCodes)BitConverter.ToUInt16(message, 2);
                sequenceNum = 0;
                body = message.Length > 4 ? message.Skip(4).ToArray() : Array.Empty<byte>();
            }
            else
            {
                code = 0;
                sequenceNum = 0;
                body = message.Length > 2 ? message.Skip(2).ToArray() : Array.Empty<byte>();
            }
        }

        public static IEnumerable<int> GetSamples(ushort sampleSize, byte[] body)
        {
            int bodySampleSize = CheckSamplesParameters(sampleSize); 
            return GetSamplesInternal(bodySampleSize, body); 
        }

        private static int CheckSamplesParameters(ushort sampleSize)
        {
            int bodySampleSize = sampleSize / 8;
            if (bodySampleSize > 4)
            {
                throw new ArgumentOutOfRangeException(nameof(sampleSize), sampleSize, "Sample size must not exceed 32 bits (4 bytes).");
            }
            return bodySampleSize;
        }

        private static IEnumerable<int> GetSamplesInternal(int bodySampleSize, byte[] body)
        {
            var bodyEnumerable = body as IEnumerable<byte>;
            var prefixBytes = Enumerable.Range(0, 4 - bodySampleSize).Select(b => (byte)0);

            while (bodyEnumerable.Count() >= bodySampleSize)
            {
                yield return BitConverter.ToInt32(bodyEnumerable.Take(bodySampleSize).Concat(prefixBytes).ToArray());
                bodyEnumerable = bodyEnumerable.Skip(bodySampleSize);
            }
        }
    }
}