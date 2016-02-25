/**
 * Original from johny3212 (https://forums.naturalpoint.com/viewtopic.php?f=59&t=10454&start=30#p57378)
 * Adapted by Matt Oskamp (https://github.com/MattOskamp/UnityOptitrack)
 * Rewritten & extended by Jan Kolkmeier (https://github.com/jankolkmeier/MocapRetargetingTools)
 */
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Text;

namespace NatNetStreaming {

    public class DirectStateObject {
        public Socket workSocket = null;
        public const int BufferSize = 65507;
        public byte[] buffer = new byte[BufferSize];
        public StringBuilder sb = new StringBuilder();
    }

    public static class NatNetSocketClient {

        private static Socket client;
        private static bool _isInitRecieveStatus = false;
        private static bool _isIsActiveThread = false;

        private static NatNetData streamData = null;
        private static String frameLog = String.Empty;

        //bool returnValue = false;
        private static int _dataPort = 1511;
        private static int _commandPort = 1510;
        private static string _multicastIPAddress = "239.255.42.99";

        private static bool receivedDataDescription = false;

        public static string localAdapter = "";


        public static int[] iData = new int[128];
        public static float[] fData = new float[512];
        public static char[] cData = new char[512];

        private static void SendDataDescriptionRequest() {
            IPEndPoint ep = new IPEndPoint(IPAddress.Parse("127.0.0.1"), _commandPort);
            UdpClient command_client = new UdpClient();
            command_client.Connect(ep);
            command_client.Send(new byte[] { 0x04, 0x00 }, 2);
            byte[] response = command_client.Receive(ref ep);
            try { 
            ReadPacket(response);
            command_client.Close();
            } catch (Exception e) {

                Debug.Log("Exception: "+e);
            }
        }

        private static void StartClient() {
            try {
                Debug.Log("[UDP] Starting client");
                streamData = new NatNetData();
                client = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
                client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
                receivedDataDescription = false;
                //client.ExclusiveAddressUse = false;

                IPEndPoint ipep = new IPEndPoint(IPAddress.Any, _dataPort);
                client.Bind(ipep);


                IPAddress ip_local = IPAddress.Any;
                if (localAdapter.Length > 0) {
                    ip_local = IPAddress.Parse(localAdapter);
                }

                IPAddress ip = IPAddress.Parse(_multicastIPAddress);
                Debug.Log("Adapter: " + ip_local);
                client.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.AddMembership, new MulticastOption(ip, ip_local));

                _isInitRecieveStatus = Receive(client);
                _isIsActiveThread = _isInitRecieveStatus;

            } catch (Exception e) {
                Debug.LogError("[UDP] DirectMulticastSocketClient: " + e.ToString());
                return;
            }

        }

        private static bool Receive(Socket client) {
            try {
                // Create the state object.
                DirectStateObject state = new DirectStateObject();
                state.workSocket = client;

                //Debug.Log("[UDP multicast] Receive");

                // Begin receiving the data from the remote device.
                client.BeginReceive(state.buffer, 0, DirectStateObject.BufferSize, 0,
                                    new AsyncCallback(ReceiveCallback), state);

            } catch (Exception e) {
                Debug.LogError(e.ToString());
                return false;
            }

            return true;
        }

        private static void ReceiveCallback(IAsyncResult ar) {
            try {
                //Debug.Log("[UDP multicast] Start ReceiveCallback");
                // Retrieve the state object and the client socket 
                // from the asynchronous state object.
                DirectStateObject state = (DirectStateObject)ar.AsyncState;
                Socket client = state.workSocket;

                // Read data from the remote device.
                int bytesRead = client.EndReceive(ar);

                if (bytesRead > 0 && _isIsActiveThread) {
                    ReadPacket(state.buffer);

                    //client.Shutdown(SocketShutdown.Both);
                    //client.Close();   

                    client.BeginReceive(state.buffer, 0, DirectStateObject.BufferSize, 0,
                                        new AsyncCallback(ReceiveCallback), state);
                } else {
                    //Debug.LogWarning("[UDP] - End ReceiveCallback");
                    if (_isIsActiveThread == false) {
                        //Debug.LogWarning("[UDP] - Closing port");
                        _isInitRecieveStatus = false;
                        //client.Shutdown(SocketShutdown.Both);
                        client.Close();
                    }
                    // Signal that all bytes have been received.
                }
            } catch (Exception e) {
                Debug.LogError(e.ToString());
            }

        }

        private static void ReadPacket(Byte[] b) {
            int offset = 0;
            int nBytes = 0;

            Buffer.BlockCopy(b, offset, iData, 0, 2); offset += 2;
            int messageID = iData[0];

            Buffer.BlockCopy(b, offset, iData, 0, 2); offset += 2;
            nBytes = iData[0];

            if (messageID == 5) {
                Debug.Log("DirectParseClient: Data descriptions");
                Buffer.BlockCopy(b, offset, iData, 0, 4); offset += 4;
                int nDatasets = iData[0];

                for (int d = 0; d < nDatasets; d++) {

                    int[] _type = new int[1]; Buffer.BlockCopy(b, offset, _type, 0, 4); offset += 4;

                    if (_type[0] == 0) { // Markerset
                        string name = "";
                        int namelen = 0;
                        while (b[offset + namelen] != 0x00) {
                            name += (char)b[offset + namelen];
                            namelen++;
                        }
                        offset = offset + namelen + 1;
                        int[] _nMarkers = new int[1]; Buffer.BlockCopy(b, offset, _nMarkers, 0, 4); offset += 4;
                        int nMarkers = _nMarkers[0];
                        //Debug.Log("Markerset: "+name+" ("+nMarkers+")"); 

                        for (int j = 0; j < nMarkers; j++) {
                            string marker_name = "";
                            int marker_namelen = 0;
                            while (b[offset + marker_namelen] != 0x00) {
                                marker_name += (char)b[offset + marker_namelen];
                                marker_namelen++;
                            }
                            offset = offset + marker_namelen + 1;
                            //Debug.Log("....with marker: "+marker_name);
                        }
                        //Debug.Log("Done");
                    } else if (_type[0] == 1) { // Rigidbody
                        string name = "";
                        int namelen = 0;
                        while (b[offset + namelen] != 0x00) {
                            name += (char)b[offset + namelen];
                            namelen++;
                        }
                        offset = offset + namelen + 1;
                        int[] _id = new int[1]; Buffer.BlockCopy(b, offset, _id, 0, 4); offset += 4;
                        int[] _parentID = new int[1]; Buffer.BlockCopy(b, offset, _parentID, 0, 4); offset += 4;
                        int id = _id[0];
                        int parentID = _parentID[0];

                        float[] _offsets = new float[3]; Buffer.BlockCopy(b, offset, _offsets, 0, 4 * 3); offset += 4 * 3;
                        Vector3 offsets = new Vector3(_offsets[0] * -1, _offsets[1], _offsets[2]);

                        NatNetRigidBody rb = null;
                        if (!streamData.rigidBodies.ContainsKey(id)) {
                            streamData.rigidBodies[id] = new NatNetRigidBody(id);
                        }
                        rb = streamData.rigidBodies[id];
                        rb.name = name;
                        rb.parentID = parentID;
                        rb.offset = offsets;

                        Debug.Log(name + ": (" + _id[0] + "," + _parentID[0] + ") [" + offsets.x + "," + offsets.y + "," + offsets.z + "]");
                    } else if (_type[0] == 2) { // Skeleton
                        string name = "";
                        while (b[offset] != 0x00) {
                            name += (char)b[offset];
                            offset++;
                        }
                        offset++;
                        //int[] _id = new int[1]; Buffer.BlockCopy(b, offset, _id, 0, 4); offset += 4;
                        int _skeletonID = BitConverter.ToInt32(b, offset); offset += 4; // Buffer.BlockCopy(b, offset, _idx, 0, 4); offset += 4;
                        int _nBones = BitConverter.ToInt32(b, offset); offset += 4;

                        NatNetSkeleton sk = null;
                        if (!streamData.skeletons.ContainsKey(_skeletonID)) {
                            streamData.skeletons[_skeletonID] = new NatNetSkeleton(_skeletonID);
                        }
                        sk = streamData.skeletons[_skeletonID];
                        sk.name = name;

                        //Debug.Log("Skeleton (ID: " + sk.ID + ", Name: "+ sk.name + ") with " + _nBones + " bones");

                        for (int i = 0; i < _nBones; i++) {
                            string boneName = "";
                            int boneNameLen = 0;
                            while (b[offset + boneNameLen] != 0x00) {
                                boneName += (char)b[offset + boneNameLen];
                                boneNameLen++;
                            }
                            offset = offset + boneNameLen + 1;

                            int boneID = BitConverter.ToInt32(b, offset); offset += 4;
                            int boneParentID = BitConverter.ToInt32(b, offset); offset += 4;
                            float[] _boneOffsets = new float[3]; Buffer.BlockCopy(b, offset, _boneOffsets, 0, 4 * 3); offset += 4 * 3;
                            Vector3 boneOffsets = new Vector3(_boneOffsets[0] * -1, _boneOffsets[1], _boneOffsets[2]);
                            NatNetRigidBody rb = null;


                            if (!sk.bones.ContainsKey(boneID)) {
                                sk.bones[boneID] = new NatNetRigidBody(boneID);
                            }
                            rb = sk.bones[boneID];
                            rb.name = boneName;
                            rb.parentID = boneParentID;
                            rb.offset = boneOffsets;
                            //Debug.Log("\t" + boneName + ": [ID=" + boneID + ", ParentID=" + boneParentID + ", Offset=[" + rb.offset.x + "," + rb.offset.y + "," + rb.offset.z + "])");

                        }
                        sk.receivedDescription = true;

                    }
                }
                receivedDataDescription = true;
                Debug.Log("End data descriptions.");
            } else if (receivedDataDescription && messageID == 7) {
                frameLog = String.Format("DirectParseClient: [UDPClient] Read FrameOfMocapData: {0}\n", nBytes);
                Buffer.BlockCopy(b, offset, iData, 0, 4); offset += 4;
                frameLog += String.Format("Frame # : {0}\n", iData[0]);

                //number of data sets (markersets, rigidbodies, etc)
                Buffer.BlockCopy(b, offset, iData, 0, 4); offset += 4;
                int nMarkerSets = iData[0];
                frameLog += String.Format("MarkerSets # : {0}\n", iData[0]);

                for (int i = 0; i < nMarkerSets; i++) {
                    String strName = "";
                    int nChars = 0;
                    while (b[offset + nChars] != '\0') {
                        nChars++;
                    }
                    strName = System.Text.Encoding.ASCII.GetString(b, offset, nChars);
                    offset += nChars + 1;

                    Buffer.BlockCopy(b, offset, iData, 0, 4); offset += 4;
                    frameLog += String.Format("{0}:" + strName + ": marker count : {1}\n", i, iData[0]);

                    nBytes = iData[0] * 3 * 4;
                    Buffer.BlockCopy(b, offset, fData, 0, nBytes); offset += nBytes;
                }

                // Other Markers - All 3D points that were triangulated but not labeled for the given frame.
                Buffer.BlockCopy(b, offset, iData, 0, 4); offset += 4;
                int nOtherMarkers = iData[0];
                frameLog += String.Format("Other Markers : {0}\n", iData[0]);
                nBytes = nOtherMarkers * 3 * 4;
                Buffer.BlockCopy(b, offset, fData, 0, nBytes); offset += nBytes;

                // Rigid Bodies
                Buffer.BlockCopy(b, offset, iData, 0, 4); offset += 4;
                int expectedRigidBodies = iData[0];
                frameLog += String.Format("Rigid Bodies : {0}\n", iData[0]);

                for (int i = 0; i < expectedRigidBodies; i++) {
                    ReadRigidBody(b, ref offset, null);
                }

                // Skeletons
                Buffer.BlockCopy(b, offset, iData, 0, 4); offset += 4;
                int expectedSkeletons = iData[0];
                frameLog += String.Format("Rigid Bodies : {0}\n", iData[0]);
                for (int i = 0; i < expectedSkeletons; i++) {
                    ReadSkeleton(b, ref offset);
                }

                // Labeled Markers
                // Force Plate data
                // latency
                // timecode
                // timestamp
                // frame params
                // end of data tag

                //Debug.Log(_strFrameLog);
            } else {
                Debug.Log("Unhandled messageID: " + messageID);
            }

            if (!receivedDataDescription) {
                SendDataDescriptionRequest();
            }

        }

        private static void ReadSkeleton(Byte[] b, ref int offset) {
            try {
                int[] _id = new int[1];
                Buffer.BlockCopy(b, offset, _id, 0, 4); offset += 4;
                int id = _id[0];

                NatNetSkeleton sk = null;
                if (!streamData.skeletons.ContainsKey(id)) {
                    streamData.skeletons[id] = new NatNetSkeleton(id);
                }
                sk = streamData.skeletons[id];

                string skName = "UNKNOWN";
                if (sk.name.Length > 0) {
                    skName = sk.name;
                }
                Buffer.BlockCopy(b, offset, iData, 0, 4); offset += 4;
                int expectedBones = iData[0];
                frameLog += String.Format("Skeleton: " + skName + " with " + expectedBones + " bones.");
                /*
                string name = "";
                while (b[offset] != 0x00) {
                    name += (char)b[offset];
                    offset++;
                }
                offset++;
                */
                for (int i = 0; i < expectedBones; i++) {
                    ReadRigidBody(b, ref offset, sk);
                }
            } catch (Exception e) {
                Debug.LogError(e.ToString());
            }
        }

        // Unpack RigidBody data
        private static void ReadRigidBody(Byte[] b, ref int offset, NatNetSkeleton sk) {
            try {
                // RB ID
                int[] _id = new int[1];
                Buffer.BlockCopy(b, offset, _id, 0, 4); offset += 4;
                int id = _id[0];

                int iSkelID = _id[0] >> 16;           // hi 16 bits = ID of bone's parent skeleton
                int iBoneID = _id[0] & 0xffff;       // lo 16 bits = ID of bone

                NatNetRigidBody rb = null;
                if (sk == null) {
                    if (!streamData.rigidBodies.ContainsKey(id)) {
                        streamData.rigidBodies[id] = new NatNetRigidBody(id);
                    }
                    rb = streamData.rigidBodies[id];
                } else if (sk.ID == iSkelID) {
                    if (!sk.bones.ContainsKey(iBoneID)) {
                        sk.bones[iBoneID] = new NatNetRigidBody(iBoneID);
                    }
                    rb = sk.bones[iBoneID];
                }

                // RB pos
                float[] pos = new float[3];
                Buffer.BlockCopy(b, offset, pos, 0, 4 * 3); offset += 4 * 3;
                rb.position.x = pos[0] * -1; rb.position.y = pos[1]; rb.position.z = pos[2];

                // RB ori
                float[] ori = new float[4];
                Buffer.BlockCopy(b, offset, ori, 0, 4 * 4); offset += 4 * 4;
                rb.rotation.x = -ori[0]; rb.rotation.y = ori[1]; rb.rotation.z = ori[2]; rb.rotation.w = -ori[3];

                // nMarkers
                int[] _nMarkers = new int[1];
                Buffer.BlockCopy(b, offset, _nMarkers, 0, 4); offset += 4;
                int nMarkers = _nMarkers[0];

                // Marker positions
                float[] _markerData = new float[3 * nMarkers];
                Buffer.BlockCopy(b, offset, _markerData, 0, 4 * 3 * nMarkers); offset += 4 * 3 * nMarkers;

                // Marker IDs
                int[] _markerIDs = new int[nMarkers];
                Buffer.BlockCopy(b, offset, _markerIDs, 0, 4 * nMarkers); offset += 4 * nMarkers;

                // Marker Sizes
                float[] _markerSizes = new float[nMarkers];
                Buffer.BlockCopy(b, offset, _markerSizes, 0, 4 * nMarkers); offset += 4 * nMarkers;

                // Marker error
                float[] _error = new float[1];
                Buffer.BlockCopy(b, offset, _error, 0, 4); offset += 4;

                // Tracking Flags
                short[] _params = new short[1];
                Buffer.BlockCopy(b, offset, _params, 0, 2); offset += 2;
                rb.tracked = (_params[0] & 0x01) > 0;
                // 0x01 : rigid body was successfully tracked in this frame
            } catch (Exception e) {
                Debug.LogError(e.ToString());
            }
        }

        public static void Start() {
            StartClient();
        }

        public static void Update() {
        }

        public static void Close() {
            _isIsActiveThread = false;
        }

        public static bool IsInit() {
            return _isInitRecieveStatus;
        }

        public static NatNetData GetStreamData() {
            return streamData;
        }
    }
}
