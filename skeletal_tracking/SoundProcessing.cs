using System;
using System.Threading.Tasks;
using NAudio.Wave;
using NAudio.Wave.SampleProviders;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class SoundCode
{
    public static double semitone = Math.Pow(2, 1.0 / 12);
    public static double upOneTone = semitone * semitone;
    public static double downOneTone = 1.0 / upOneTone;

    private static ISampleProvider input;
    private static SmbPitchShiftingSampleProvider pitchStream;
    private static WaveOutEvent device;

    //Used for mic input
    private static BufferedWaveProvider bwp;

    //Used for sine and trig waves
    public static double setFreq;

    static public void InitialiseFileStream(String inPath)
    {
        MediaFoundationReader reader = new MediaFoundationReader(inPath);
        input = new SmbPitchShiftingSampleProvider(reader.ToSampleProvider());
    }

    static public void InitialiseMic()
    {
        WaveInEvent wi = new WaveInEvent
        {
            WaveFormat = new WaveFormat(44100, 1)
        };

        wi.DataAvailable += new EventHandler<WaveInEventArgs>(wi_DataAvailable);

        bwp = new BufferedWaveProvider(wi.WaveFormat)
        {
            DiscardOnBufferOverflow = true
        };
        input = bwp.ToSampleProvider();
        wi.StartRecording();
    }

    static void wi_DataAvailable(object sender, WaveInEventArgs e)
    {
        bwp.AddSamples(e.Buffer, 0, e.BytesRecorded);
    }

    static public void GenerateSineWave()
    {
        var sine = new SignalGenerator()
        {
            Gain = 0.2,
            Frequency = setFreq,
            Type = SignalGeneratorType.Sin
        };

        input = sine;
    }


    static public void GenerateTrigWave()
    {
        var sine = new SignalGenerator()
        {
            Gain = 0.2,
            Frequency = setFreq,
            Type = SignalGeneratorType.Triangle
        };

        input = sine;
    }

    static public void GenerateSquareWave()
    {
        var square = new SignalGenerator()
        {
            Gain = 0.2,
            Frequency = setFreq,
            Type = SignalGeneratorType.Square
        };

        input = square;
    }

    static public void GenerateSawToothWave()
    {
        var sawTooth = new SignalGenerator()
        {
            Gain = 0.2,
            Frequency = setFreq,
            Type = SignalGeneratorType.SawTooth
        };

        input = sawTooth;
    }

    static public void GenerateSweepWave()
    {
        var sweep = new SignalGenerator()
        {
            Gain = 0.2,
            Frequency = setFreq,
            Type = SignalGeneratorType.Sweep
        };

        input = sweep;
    }

    static private async Task ChangePitch()
    {
        pitchStream = new SmbPitchShiftingSampleProvider(input);

        using (device = new WaveOutEvent())
        {
            device.Init(pitchStream);
            device.Play();
            while (device.PlaybackState == PlaybackState.Playing)
            {
                await Task.Delay(2000);
            }
        }
    }

    static public async Task ReproduceSound()
    {
        await ChangePitch();
    }

    static public void ChangePitch(float pitch = 1.0f)
    {
        pitchStream.PitchFactor = pitch;
    }

    static public void ChangeVolume(float volume = 0.5f)
    {
        device.Volume = volume;
    }
}