/*
Copyright 2019 LIV inc.

Permission is hereby granted, free of charge, to any person obtaining a copy of this software 
and associated documentation files (the "Software"), to deal in the Software without restriction, 
including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, 
and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, 
subject to the following conditions:

The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.
THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED 
TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. 
IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, 
WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE 
OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
*/

using System.Collections;
using System.Collections.Generic;
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using UnityEngine;

// User defined settings which will be serialized and deserialized with Newtonsoft Json.Net.
// Only public variables will be serialized.
public class ThirdPersonCameraPluginSettings : IPluginSettings {
    public float fov = 60f;
    public float distance = 2f;
    public float speed = 1f;
    
    public string foveServerIP = "127.0.0.1"; // DON'T FORGET TO INSTALL OBS PYTHON SCRIPT OR DISABLE FOV EFFECT!
    public int foveServerPort = 50734; // 50734 = FOVEA
}

// The class must implement IPluginCameraBehaviour to be recognized by LIV as a plugin.
public class ThirdPersonCameraPlugin : IPluginCameraBehaviour {

    // Store your settings localy so you can access them.
    ThirdPersonCameraPluginSettings _settings = new ThirdPersonCameraPluginSettings();

    // Provide your own settings to store user defined settings .   
    public IPluginSettings settings => _settings;

    // Invoke ApplySettings event when you need to save your settings.
    // Do not invoke event every frame if possible.
    public event EventHandler ApplySettings;

    // ID is used for the camera behaviour identification when the behaviour is selected by the user.
    // It has to be unique so there are no plugin collisions.
    public string ID => "ThirdPersonCameraPlugin";
    // Readable plugin name "Keep it short".
    public string name => "Third Person Camera";
    // Author name.
    public string author => "Mystery";
    // Plugin version.
    public string version => "1.0";
    // Localy store the camera helper provided by LIV.

    PluginCameraHelper _helper;
    float _elaspedTime;

    private TcpListener foveServer = null;
    private float currentFOVEDistanceMultipler = 1f;

    // Constructor is called when plugin loads
    public ThirdPersonCameraPlugin() { }

    // OnActivate function is called when your camera behaviour was selected by the user.
    // The pluginCameraHelper is provided to you to help you with Player/Camera related operations.
    public void OnActivate(PluginCameraHelper helper) {
        _helper = helper;
    }
    
    private void StartFOVEServer()
    {
        try
        {
//            debug("[FOVEServer]: Starting...");
            IPAddress localAddr = IPAddress.Parse(_settings.foveServerIP);
            foveServer = new TcpListener(localAddr, _settings.foveServerPort);
 
            // start listener
            foveServer.Start();
 
            while (true)
            {
//                debug("[FOVEServer]: Awaiting for connections...");
 
                // recieve incoming connection
                TcpClient client = foveServer.AcceptTcpClient();
//                debug("[FOVEServer]: A client has connected.");
 
                // recieve network stream for read/write operations
                NetworkStream stream = client.GetStream();
 
                string response = FormatNumDigits(currentFOVEDistanceMultipler, 8).Substring(1);
                // encode response as a byte array
                byte[] data = Encoding.UTF8.GetBytes(response);
 
                stream.Write(data, 0, data.Length);
//                debug("[FOVEServer]: response: " + response);

                stream.Close();
                client.Close();
            }
        }
        catch (Exception e)
        {
//            debug("[FOVEServer]: ERROR: " + e.Message);
        }
        finally
        {
//            debug("[FOVEServer]: Stopping.");
            foveServer?.Stop();
        }
    }
    
    // This method was copied from stackoverflow 'cause why not~
    // https://stackoverflow.com/questions/11789194/string-format-how-can-i-format-to-x-digits-regardless-of-decimal-place
    public static string FormatNumDigits(float number, int x) {
        string asString = (number >= 0? "+":"") + number.ToString("F50",System.Globalization.CultureInfo.InvariantCulture);

        if (asString.Contains(".")) {
            if (asString.Length > x + 2) {
                return asString.Substring(0, x + 2);
            } else {
                // Pad with zeros
                return asString.Insert(asString.Length, new String('0', x + 2 - asString.Length));
            }
        } else {
            if (asString.Length > x + 1) {
                return asString.Substring(0, x + 1);
            } else {
                // Pad with zeros
                return asString.Insert(1, new String('0', x + 1 - asString.Length));
            }
        }
    }

    // OnSettingsDeserialized is called only when the user has changed camera profile or when the.
    // last camera profile has been loaded. This overwrites your settings with last data if they exist.
    public void OnSettingsDeserialized() {

    }

    // OnFixedUpdate could be called several times per frame. 
    // The delta time is constant and it is ment to be used on robust physics simulations.
    public void OnFixedUpdate() {

    }

    // OnUpdate is called once every frame and it is used for moving with the camera so it can be smooth as the framerate.
    // When you are reading other transform positions during OnUpdate it could be possible that the position comes from a previus frame
    // and has not been updated yet. If that is a concern, it is recommended to use OnLateUpdate instead.
    public void OnUpdate() {
        _elaspedTime += Time.deltaTime * _settings.speed;
        Transform headTransform = _helper.playerHead;

        ProcessFOVEMultipler();

        Vector3 rotationVector = headTransform.forward * _settings.distance;
        Vector3 targetCameraPosition = headTransform.position - rotationVector;
        Quaternion targetCameraRotation = Quaternion.LookRotation(headTransform.forward);
        _helper.UpdateCameraPose(targetCameraPosition, targetCameraRotation);
        _helper.UpdateFov(_settings.fov);
    }

    private void ProcessFOVEMultipler()
    {
        currentFOVEDistanceMultipler = 1 + (_elaspedTime * 7) % 30;
    }

    // OnLateUpdate is called after OnUpdate also everyframe and has a higher chance that transform updates are more recent.
    public void OnLateUpdate() {

    }

    // OnDeactivate is called when the user changes the profile to other camera behaviour or when the application is about to close.
    // The camera behaviour should clean everything it created when the behaviour is deactivated.
    public void OnDeactivate() {
        ApplySettings?.Invoke(this, EventArgs.Empty);
    }

    // OnDestroy is called when the users selects a camera behaviour which is not a plugin or when the application is about to close.
    // This is the last chance to clean after your self.
    public void OnDestroy() {

    }
}
