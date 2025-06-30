using System;

struct PcmData
{
    #region Public types & data

    public float[] Value      { get; }
    public int     Length     { get; }
    public int     Channels   { get; }
    public int     SampleRate { get; }

    #endregion

    #region Constructors & Finalizer

    private PcmData(float[] value, int channels, int sampleRate)
    {
        Value      = value;
        Length     = value.Length;
        Channels   = channels;
        SampleRate = sampleRate;
    }

    #endregion

    #region Public Methods

    public static PcmData FromBytes(byte[] bytes)
    {
        if (bytes == null)
        {
            throw new ArgumentNullException(nameof(bytes));
        }

        PcmHeader pcmHeader = PcmHeader.FromBytes(bytes);
        if (pcmHeader.BitDepth != 16 && pcmHeader.BitDepth != 32 && pcmHeader.BitDepth != 8)
        {
            throw new ArgumentOutOfRangeException(nameof(pcmHeader.BitDepth), pcmHeader.BitDepth, "Supported values are: 8, 16, 32");
        }

        float[] samples = new float[pcmHeader.AudioSampleCount];
        for (int i = 0; i < samples.Length; ++i)
        {
            int   byteIndex = pcmHeader.AudioStartIndex + i * pcmHeader.AudioSampleSize;
            float rawSample;
            switch (pcmHeader.BitDepth)
            {
                case 8:
                    rawSample = bytes[byteIndex];
                    break;

                case 16:
                    rawSample = BitConverter.ToInt16(bytes, byteIndex);
                    break;

                case 32:
                    rawSample = BitConverter.ToInt32(bytes, byteIndex);
                    break;

                default: throw new ArgumentOutOfRangeException(nameof(pcmHeader.BitDepth), pcmHeader.BitDepth, "Supported values are: 8, 16, 32");
            }

            samples[i] = pcmHeader.NormalizeSample(rawSample); // normalize sample between [-1f, 1f]
        }

        return new PcmData(samples, pcmHeader.Channels, pcmHeader.SampleRate);
    }

    #endregion
}