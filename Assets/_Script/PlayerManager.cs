using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ETLab
{
    /// <summary>
    /// 透過 PlayerManager 在各場景間保留 Player，並提供 DetectManager 來存取
    /// </summary>
    public class PlayerManager : MonoBehaviour
    {
        public Player[] players;

        // Start is called before the first frame update
        void Start()
        {
            DontDestroyOnLoad(this);

            // TODO: 根據實際人數，調整出現的模型數量 
            
        }

        // Update is called once per frame
        void Update()
        {

        }

        public int getPlayerNumber()
        {
            // TODO: 根據實際人數，調整出現的模型數量 
            return players.Length;
        }

        public Player[] getPlayers()
        {
            // TODO: 根據實際人數，調整出現的模型數量 
            return players;
        }

        public Player getPlayer(int index)
        {
            // TODO: 根據實際人數，調整出現的模型數量 
            return players[index];
        }
    }
}
