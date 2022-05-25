using UnityEngine;
using UnityEngine.UI;
using Reliable;
using Cereal;
using System.Threading;
using System.IO;

public class Move : MonoBehaviour
{
    Rigidbody rb;
    HingeJoint hj;
    Text t;

    ISocket Socket;
    CallbackClient RelClient;
    PerPacketTimer Timer;
    AngleRecorder Angles;

    float nextForce = 0.0f;

    bool Pushed = false;
    float Push = 0;

    // Start is called before the first frame update
    void Start()
    {
        Push = float.Parse(GetArg("-push") ?? "0.0");

        rb = gameObject.GetComponent<Rigidbody>();
        hj = GameObject.Find("Pole").GetComponent<HingeJoint>();
        t = gameObject.GetComponent<Text>();

        Angles = new AngleRecorder();
        Timer = new PerPacketTimer();

        Socket = new TaskUdpSocket(8010);

        void applyForce(float force)
        {
            nextForce = force;
        }

        RelClient = new PrintedCallbackClient(Socket, applyForce, Timer, UnityEngine.Debug.Log);

        string ip = GetArg("-single");
        if (ip != null)
        {
            RelClient.Connect(ip, 8010);
        } 
        else
        {
            RelClient.Connect("10.2.1.149", 8010);
            RelClient.Connect("10.2.1.150", 8010);
        }

        Thread.Sleep(500);
    }

    // Update is called once per frame
    void Update()
    {
        float mass = rb.mass;
        float f = 1 * mass;

        if (Input.GetKey(KeyCode.A))
        {
            rb.AddForce(-f, 0, 0);
        }

        if (Input.GetKey(KeyCode.D))
        {
            rb.AddForce(f, 0, 0);
        }

        if (!Pushed)
        {
            rb.AddForce(Push * rb.mass, 0, 0);
            Pushed = true;
        }
    }

    void FixedUpdate()
    {
        rb.AddForce(nextForce, 0, 0);
        nextForce = 0;
        Angles.RecordNew(hj.angle);
        RelClient.Send(hj.angle);
        Debug.Log(hj.angle);
    }

    void OnApplicationQuit()
    {
        string s = "";
        for (int i = 0; i < 60; i++)
        {
            s += Timer.Times[i] + " ";
        }
        UnityEngine.Debug.Log(s);

        UnityEngine.Debug.Log(Angles.GetValues().Count);

        string prefix = GetArg("-outputPrefix") ?? "";
        if (prefix.Length > 0) prefix += "-";

        FileWriter.SaveList(Timer.Times, $"{prefix}times.json");
        FileWriter.DicToJson(Angles.GetValues(), $"{prefix}angles.json");

        string[] save = { RelClient.SrcCounter.Addresses.Str(), RelClient.SrcCounter.Counts.Str() };
        File.WriteAllLines($"{prefix}srccounts.txt", save);
    }

    private string GetArg(string name)
    {
        var args = System.Environment.GetCommandLineArgs();
        for (int i = 0; i < args.Length; i++)
        {
            if (args[i] == name && args.Length > i + 1)
            {
                return args[i + 1];
            }
        }
        return null;
    }
}
