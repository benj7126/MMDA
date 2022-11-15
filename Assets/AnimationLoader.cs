using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationLoader : MonoBehaviour
{
    public List<string> listOfDances = default;

    private Dictionary<string, Dictionary<string, Dictionary<uint, rotNPos>>> dances = new Dictionary<string, Dictionary<string, Dictionary<uint, rotNPos>>>();
    // Start is called before the first frame update
    void Start()
    {
        string myPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "MMDAFiles");
        loadAllFromPath(Path.Combine(myPath, "MMDFiles"));
    }

    public Dictionary<string, rotNPos> getBoneMovementAtFrame(string danceName, uint frame)
    {
        Dictionary<string, rotNPos> retDic = new Dictionary<string, rotNPos>();

        /*
        Debug.Log("stuff");
        foreach (KeyValuePair<string, Dictionary<string, Dictionary<uint, rotNPos>>> b in dances) // its empty...
        {
            Debug.Log(b.Key);
        } 
        */

        foreach (KeyValuePair<string, Dictionary<uint, rotNPos>> bone in dances[danceName])
        {
            rotNPos closestPos = default;
            rotNPos closestNeg = default;

            int closestPosFrame = 99^99;
            int closestNegFrame = -99^99;

            rotNPos endRes = default;

            foreach (KeyValuePair<uint, rotNPos> frameKPV in bone.Value)
            {
                int diff = (int)frame - (int)frameKPV.Key;
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
                    if (diff > closestNegFrame)
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

            if (closestPosFrame != 0 && closestNegFrame != 0 && closestPosFrame != (99^99) && closestNegFrame != (-99^99))
            {
                // idk what i do here...
                float posScale = (float)closestPosFrame / (closestPosFrame + Math.Abs(closestNegFrame));

                endRes = closestPos;

            }

            if (closestPosFrame == (99 ^ 99) || closestNegFrame == (-99 ^ 99))
            {
                if (closestPosFrame == (99 ^ 99))
                    endRes = closestPos;
                else
                    endRes = closestNeg;
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

                Dictionary<string, string> translationList = getTranslationList(folderParent);

                dances.Add(folderParent.Name, getMotion(bytes, translationList));
            }
        }
    }

    Dictionary<string, string> getTranslationList(DirectoryInfo folderParent)
    {
        Dictionary<string, string> retDic = new Dictionary<string, string>();

        foreach (FileInfo file in folderParent.GetFiles())
        {
            if (file.Name == "TranslationList.txt")
            {
                foreach (string s in File.ReadAllLines(file.FullName))
                {
                    if (s[0] == '#')
                        continue;

                    string[] strings = s.Split('>');

                    if (strings.Length > 1)
                        continue;
                    
                    retDic.Add(strings[0], strings[1]);
                }
            }
        }

        return retDic;
    }

    Dictionary<string, Dictionary<uint, rotNPos>> getMotion(byte[] bytes, Dictionary<string, string> transList)
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
            string tempBoneName = System.Text.Encoding.GetEncoding("shift-jis").GetString(getAndPushBytes(bytes, ref arrayPosition, 15), 0, 15);

            string boneName = "";
            
            foreach (char c in tempBoneName)
            {
                if (c == 0x00)
                    break;
                
                boneName += c;
            }

            if (transList.ContainsKey(boneName))
                boneName = transList[boneName];
            
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
                getCopy = getCopy + boneName + ">\n"; // dosent work for some reason...
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

        //Debug.Log(getCopy);
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
