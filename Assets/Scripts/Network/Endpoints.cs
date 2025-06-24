using System;
using UnityEngine;

public enum Endpoints
{
    Login,
    CurrentUserLogin,
    Logout,
    Locale,
    MyProfile,
    MyPicture,
    ProfilePicture,
    UploadPicture,
    UploadMyProfile,
    Parties,
    Modules,
    Friends,
    FriendPicture,
    AddFriend,
    Agenda,
    AgendaFile,
    Documents,
    Activities
}

[Serializable]
public class EndpointInfo
{
    public string Path;
    public string ApiVerb;
}