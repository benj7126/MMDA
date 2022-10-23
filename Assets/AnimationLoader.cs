using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationLoader : MonoBehaviour
{
    public List<string> listOfDances = default;

    private Dictionary<string, Dictionary<string, Dictionary<uint, rotNPos>>> dances = default;
    // Start is called before the first frame update
    void Start()
    {
        string myPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "MMDAFiles");
        loadFile(Path.Combine(myPath, "MMDFiles"));
    }

    public Dictionary<string, rotNPos> getBoneMovementAtFrame(string danceName, uint frame)
    {
        Dictionary<string, rotNPos> retDic = default;

        foreach (KeyValuePair<string, Dictionary<uint, rotNPos>> bone in dances[danceName])
        {
            rotNPos closestPos;
            rotNPos closestNeg;

            int closestPosFrame = 99^99;
            int closestNegFrame = -99^99;

            rotNPos endRes;

            foreach (KeyValuePair<uint, rotNPos> frameKPV in bone.Value)
            {
                uint diff = frame - frameKPV.Key;
                if (diff > 0) // change pos(positive) frame
                {
                    if (diff < closestPosFrame)
                    {
                        closestPosFrame = (int)diff;
                        closestPos = frameKPV.Value;
                    }
                }
                else if (diff < 0) // change neg(negative) frame
                {
                    if (diff > closestPosFrame)
                    {
                        closestNegFrame = (int)diff;
                        closestNeg = frameKPV.Value;
                    }
                }
                else // the exact frame...
                {
                    closestPosFrame = 0;
                    closestNegFrame = 0;

                    endRes = frameKPV.Value;
                }
            }

            if (closestPosFrame != 0 && closestNegFrame != 0)
            {
                // idk what i do here...

                float posScale = (float)closestPosFrame/(closestPosFrame + Math.Abs(closestNegFrame));

            }
        }



        return retDic;
    }

    public struct rotNPos
    {
        public Vector3 vec;
        public Quaternion rot;
    }

    void loadFile(string path)
    {
        Debug.Log("f");

        DirectoryInfo info = new DirectoryInfo(path);
        FileInfo[] fileInfo = info.GetFiles();
        foreach (FileInfo file in fileInfo)
        {
            Dictionary<string, Dictionary<uint, rotNPos>> frames = new Dictionary<string, Dictionary<uint, rotNPos>>();

            int arrayPosition = 0;
            byte[] bytes = readFile(file.FullName);

            string start = System.Text.Encoding.UTF8.GetString(getAndPushBytes(bytes, ref arrayPosition, 30), 0, 30);

            Debug.Log("----------------------------------------------------------------- start new thing");

            // check if it is the new or old version of MMD
            bool newVersion = false;
            if (start.Substring(0, "Vocaloid Motion Data 0002".Length) == "Vocaloid Motion Data 0002")
                newVersion = true;

            arrayPosition += 10;
            if (newVersion)
                arrayPosition += 10;


            Debug.Log(arrayPosition + " | " + (start.Substring(0, "Vocaloid Motion Data 0002".Length) == "Vocaloid Motion Data 0002"));

            uint howManyKeyframes = BitConverter.ToUInt32(getAndPushBytes(bytes, ref arrayPosition, 4));

            Debug.Log("Frames: " + howManyKeyframes);

            for (int keyframeIndex = 0; keyframeIndex < howManyKeyframes; keyframeIndex++)
            {
                string boneName = System.Text.Encoding.GetEncoding("shift-jis").GetString(getAndPushBytes(bytes, ref arrayPosition, 15), 0, 15);
                uint frame = BitConverter.ToUInt32(getAndPushBytes(bytes, ref arrayPosition, 4));

                float xP = BitConverter.ToSingle(getAndPushBytes(bytes, ref arrayPosition, 4));
                float yP = BitConverter.ToSingle(getAndPushBytes(bytes, ref arrayPosition, 4));
                float zP = BitConverter.ToSingle(getAndPushBytes(bytes, ref arrayPosition, 4));

                float xR = BitConverter.ToSingle(getAndPushBytes(bytes, ref arrayPosition, 4));
                float yR = BitConverter.ToSingle(getAndPushBytes(bytes, ref arrayPosition, 4));
                float zR = BitConverter.ToSingle(getAndPushBytes(bytes, ref arrayPosition, 4));
                float wR = BitConverter.ToSingle(getAndPushBytes(bytes, ref arrayPosition, 4));

                if (!frames.ContainsKey(boneName))
                {
                    Debug.Log("Add bone: " + boneName);
                    frames.Add(boneName, new Dictionary<uint, rotNPos>());
                }

                rotNPos thisRNP = new rotNPos();
                thisRNP.vec = new Vector3(xP, yP, zP);
                thisRNP.rot = new Quaternion(xR, yR, zR, wR);

                frames[boneName].Add(frame, thisRNP);

                //skip frame interpolation data (i have no clue how it works/what it dose, so its basically useless...)
                arrayPosition += 64;
            }

            listOfDances.Add(file.Name);
            dances.Add(file.Name, frames);
            Debug.Log("totalling in: " + frames.Count);
        }
    }

    byte[] getAndPushBytes(byte[] bytes, ref int bytePos, int bytesToReturn)
    {
        byte[] toReturn = bytes[bytePos..(bytePos + bytesToReturn)];
        bytePos += bytesToReturn; // + 1;
        return toReturn;
    }

    byte[] readFile(string path)
    {
        return File.ReadAllBytes(path);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
