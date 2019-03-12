using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SquadFightersServer
{
    public enum ServerMethod
    {
        None,
        StartDownloadMapData,
        MapDataDownloadCompleted,
        PlayerConnected,
        RemoveItem,
        UpdateItemCapacity,
        ShootData,
        Revive,
        PlayerData,
        DownloadingItem,
        JoinedMatch,
        PlayerKilled,
        PlayerDisconnected
    }
}
