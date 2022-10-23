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
        loadAllFromPath(Path.Combine(myPath, "MMDFiles"));
    }

    public Dictionary<string, rotNPos> getBoneMovementAtFrame(string danceName, uint frame)
    {
        Dictionary<string, rotNPos> retDic = default;

        foreach (KeyValuePair<string, Dictionary<uint, rotNPos>> bone in dances[danceName])
        {
            rotNPos closestPos = default;
            rotNPos closestNeg = default;

            int closestPosFrame = 99^99;
            int closestNegFrame = -99^99;

            rotNPos endRes = default;

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
                float posScale = (float)closestPosFrame / (closestPosFrame + Math.Abs(closestNegFrame));

                endRes = closestPos;

            }

            retDic.Add(bone.Key, endRes);
        }

        return retDic;
    }

    public struct rotNPos
    {
        public Vector3 vec;
        public Quaternion rot;
    }

    void loadAllFromPath(string path)
    {
        DirectoryInfo info = new DirectoryInfo(path);
        DirectoryInfo[] fileInfo = info.GetDirectories();

        foreach (DirectoryInfo file in fileInfo)
        {
            Debug.Log(file.Name);
            addDance(file);
        }
    }

    void addDance(DirectoryInfo folderParent)
    {
        // make a dance object, and have the motion in it, but for now, dont...

        foreach (FileInfo file in folderParent.GetFiles())
        {
            if (file.Extension.ToLower() == ".vmd")
            {
                byte[] bytes = readFile(file.FullName);

                listOfDances.Add(folderParent.Name);

                Dictionary<string, Dictionary<uint, rotNPos>> motion = getMotion(bytes);
                
                dances.Add(folderParent.Name, motion);
            }
        }
    }

    Dictionary<string, Dictionary<uint, rotNPos>> getMotion(byte[] bytes)
    {
        Dictionary<string, Dictionary<uint, rotNPos>> frames = new Dictionary<string, Dictionary<uint, rotNPos>>();

        int arrayPosition = 0;

        string start = System.Text.Encoding.UTF8.GetString(getAndPushBytes(bytes, ref arrayPosition, 30), 0, 30);


        // check if it is the new or old version of MMD
        bool newVersion = false;
        if (start.Substring(0, "Vocaloid Motion Data 0002".Length) == "Vocaloid Motion Data 0002")
            newVersion = true;

        arrayPosition += 10;
        if (newVersion)
            arrayPosition += 10;

        string getCopy = "";


        //Debug.Log(arrayPosition + " | " + (start.Substring(0, "Vocaloid Motion Data 0002".Length) == "Vocaloid Motion Data 0002"));

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
                //Debug.Log("Add bone: " + boneName);
                getCopy = getCopy + boneName + ">\n";
                frames.Add(boneName, new Dictionary<uint, rotNPos>());
            }

            rotNPos thisRNP = new rotNPos();
            thisRNP.vec = new Vector3(xP, yP, zP);
            thisRNP.rot = new Quaternion(xR, yR, zR, wR);

            frames[boneName].Add(frame, thisRNP);

            //skip frame interpolation data (i have no clue how it works/what it dose, so its basically useless...)
            arrayPosition += 64;
        }

        GUIUtility.systemCopyBuffer = getCopy;

        //Debug.Log("totalling in: " + frames.Count);
        return frames;
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
