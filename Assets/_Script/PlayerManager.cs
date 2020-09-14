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
        private int n_player;
        Player[] read_players;

        private void Awake()
        {
            Utils.log();

            // 根據實際人數，調整出現的模型數量 
            n_player = players.Length;
            read_players = new Player[n_player];
            for (int i = 0; i < n_player; i++)
            {
                read_players[i] = players[i];
            }
        }

        // Start is called before the first frame update
        void Start()
        {
            Utils.log();
            DontDestroyOnLoad(this);
        }

        // Update is called once per frame
        void Update()
        {

        }

        // 根據實際人數，調整出現的模型數量 
        public void init(int n_player)
        {
            Debug.Log(string.Format("[PlayerManager] init(n_player: {0})", n_player));
            this.n_player = n_player;
            read_players = new Player[n_player];
            for (int i = 0; i < n_player; i++)
            {
                read_players[i] = players[i];
            }

            for (int i = n_player; i < players.Length; i++)
            {
                players[i].gameObject.SetActive(false);
            }
        }

        public int getPlayerNumber()
        {
            return n_player;
        }

        public Player[] getPlayers()
        {
            return read_players;
        }

        public Player getPlayer(int index)
        {
            if(index < n_player)
            {
                return players[index];
            }

            Debug.LogError(string.Format("[PlayerManager] getPlayer | n_player: {0} <= index: {1} ", n_player, index));

            return null;
        }
    }
}
