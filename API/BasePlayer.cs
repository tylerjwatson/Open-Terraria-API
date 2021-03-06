﻿using Microsoft.Xna.Framework;
using System;
using System.Collections.Concurrent;
using OTA;
using OTA.Command;

#if Full_API
using Terraria.Net.Sockets;
#endif

namespace OTA
{
    /// <summary>
    /// This is what Terraria.Player is forced to inherit
    /// </summary>
    public partial class BasePlayer : Sender
    {
        /// <summary>
        /// Gets or sets the clients globally unique identifier.
        /// </summary>
        public string ClientUUId { get; set; }

        /// <summary>
        /// Gets who the player is authenticated as
        /// </summary>
        /// <value>The authenticated as.</value>
        public string AuthenticatedAs { get; private set; }

        /// <summary>
        /// Gets the authenticator
        /// </summary>
        public string AuthenticatedBy { get; private set; }

        /// <summary>
        /// Gets a value indicating whether this player is authenticated.
        /// </summary>
        /// <value><c>true</c> if this instance is authenticated; otherwise, <c>false</c>.</value>
        public bool IsAuthenticated
        {
            get { return !String.IsNullOrEmpty(AuthenticatedAs) && AuthenticatedBy != Name; }
        }

        /// <summary>
        /// Gets a value indicating whether this player is self authenticated.
        /// </summary>
        /// <remarks>This is not for real authentication, rather to prevent self authentications being included</remarks>
        public bool IsSelfAuthenticated
        {
            get { return AuthenticatedBy == Name; }
        }

        /// <summary>
        /// Storage for plugins for on a per-player basis
        /// </summary>
        public ConcurrentDictionary<String, Object> PluginData = new ConcurrentDictionary<String, Object>();

        /// <summary>
        /// Sets plugin data for this player
        /// </summary>
        /// <param name="key">Key.</param>
        /// <param name="value">Value.</param>
        public void SetPluginData(string key, object value)
        {
            if (PluginData == null)
                PluginData = new System.Collections.Concurrent.ConcurrentDictionary<String, Object>();
            PluginData[key] = value;
        }

        /// <summary>
        /// Gets plugin data for this player
        /// </summary>
        /// <returns>The plugin data.</returns>
        /// <param name="key">Key.</param>
        /// <param name="defaultValue">Default value.</param>
        /// <typeparam name="T">The 1st type parameter.</typeparam>
        public T GetPluginData<T>(string key, T defaultValue)
        {
            if (PluginData == null)
            {
                PluginData = new System.Collections.Concurrent.ConcurrentDictionary<String, Object>();
            }
            else if (PluginData.ContainsKey(key))
            {
                return (T)(PluginData[key] ?? defaultValue);
            }
            return defaultValue;
        }

        /// <summary>
        /// Removed plugin data for this player
        /// </summary>
        /// <returns><c>true</c>, if plugin data was cleared, <c>false</c> otherwise.</returns>
        /// <param name="key">Key.</param>
        public bool ClearPluginData(string key)
        {
            if (PluginData == null)
            {
                PluginData = new System.Collections.Concurrent.ConcurrentDictionary<String, Object>();
            }
            if (PluginData.ContainsKey(key))
            {
                object val;
                return PluginData.TryRemove(key, out val);
            }
            return false;
        }

        /// <summary>
        /// Sets the authentication of this user
        /// </summary>
        /// <param name="auth">Auth.</param>
        /// <param name="by">By.</param>
        public void SetAuthentication(string auth, string by)
        {
            #if Full_API
            var ctx = new Plugin.HookContext()
            {
                Player = this as Terraria.Player,
                Connection = this.Connection.Socket
            };
            var changing = new Plugin.HookArgs.PlayerAuthenticationChanging()
            {
                AuthenticatedAs = auth,
                AuthenticatedBy = by
            };

            Plugin.HookPoints.PlayerAuthenticationChanging.Invoke(ref ctx, ref changing);
            if (ctx.Result != Plugin.HookResult.DEFAULT)
                return;

            this.AuthenticatedAs = auth;
            this.AuthenticatedBy = by;

            ctx = new Plugin.HookContext()
            {
                Player = this as Terraria.Player,
                Connection = this.Connection.Socket
            };
            var changed = new Plugin.HookArgs.PlayerAuthenticationChanged()
            {
                AuthenticatedAs = this.AuthenticatedAs,
                AuthenticatedBy = this.AuthenticatedBy
            };

            Plugin.HookPoints.PlayerAuthenticationChanged.Invoke(ref ctx, ref changed);
            #endif
        }

        /// <summary>
        /// Gets the name of this player
        /// </summary>
        /// <returns>Sending entity name</returns>
        /// <value>The name.</value>
        /// <remarks>This is for compatibility</remarks>
        public override string Name
        {
            get
            {
#if Full_API
                if (this is Terraria.Player)
                {
                    return ((Terraria.Player)this).name;
                }
#endif
                return "??";
            }
            protected set { }
        }

        /// <summary>
        /// Sends a message to the players client
        /// </summary>
        /// <param name="Message">Message to send</param>
        /// <param name="A"></param>
        /// <param name="R">Red text color value</param>
        /// <param name="G">Green text color value</param>
        /// <param name="B">Blue text color value</param>
        /// <param name="message">Message.</param>
        /// <param name="sender">Sender.</param>
        public override void SendMessage(string message, int sender = 255, byte R = 255, byte G = 255, byte B = 255)
        {
#if Full_API
            Terraria.NetMessage.SendData((int)Packet.PLAYER_CHAT, ((Terraria.Player)this).whoAmI, -1, message, sender, R, G, B);
#endif
        }

        /// <summary>
        /// Sends a message to the player
        /// </summary>
        /// <param name="message">Message.</param>
        /// <param name="color">Color.</param>
        public void SendMessage(string message, Color color)
        {
            SendMessage(message, 255, color.R, color.G, color.B);
        }

        #if Full_API
        /// <summary>
        /// Gets the players connection instance.
        /// </summary>
        /// <value>The connection.</value>
        public Terraria.RemoteClient Connection
        {
            get { return Terraria.Netplay.Clients[this.whoAmI]; }
        }
        #endif

        /// <summary>
        /// The players IP Address
        /// </summary>
        public string IPAddress;

        /// <summary>
        /// The reason of disconnection
        /// </summary>
        public string DisconnectReason;

        #if Full_API
        /// <summary>
        /// Teleports a player to another player
        /// </summary>
        /// <param name="target"></param>
        /// <param name="style"></param>
        public void Teleport(Terraria.Player target, int style = 0)
        {
            if (this is Terraria.Player)
            {
                var plr = (Terraria.Player)this;

                Terraria.RemoteClient.CheckSection(plr.whoAmI, target.position);
                plr.Teleport(target.position, style);
                Terraria.NetMessage.SendData((int)Packet.TELEPORT, -1, -1, "", 0, plr.whoAmI, target.position.X, target.position.Y, 3);
            }
        }

        /// <summary>
        /// Teleports a player to a specified location
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="style"></param>
        public void Teleport(float x, float y, int style = 0)
        {
            if (this is Terraria.Player)
            {
                var plr = (Terraria.Player)this;
                var pos = new Vector2(x, y);

                Terraria.RemoteClient.CheckSection(plr.whoAmI, pos);
                plr.Teleport(pos, style);
                Terraria.NetMessage.SendData((int)Packet.TELEPORT, -1, -1, "", 0, plr.whoAmI, pos.X, pos.Y, 3);
            }
        }

        /// <summary>
        /// Removes a player from the server
        /// </summary>
        /// <param name="reason"></param>
        /// <param name="announce"></param>
        public void Kick(string reason)
        {
            Connection.Kick(reason);
        }

        /// <summary>
        /// Gives a player an item
        /// </summary>
        /// <param name="itemId"></param>
        /// <param name="stack"></param>
        /// <param name="sender"></param>
        /// <param name="netId"></param>
        /// <param name="notifyOps"></param>
        /// <param name="prefix"></param>
        /// <returns></returns>
        public int GiveItem(int itemId, int stack, int maxStack, ISender sender, int netId, bool notifyOps = true, int prefix = 0)
        {
            if (this is Terraria.Player)
            {
                var plr = (Terraria.Player)this;

                // Set a max drops limit to be safe.
                int maxDrops = 10;
                if (stack / maxStack > maxDrops)
                {
                    stack = maxStack * maxDrops;
                }

                int index;
                while (stack > maxStack) // If stack is greater than the stack size...
                {
                    index = Terraria.Item.NewItem((int)plr.position.X, (int)plr.position.Y, plr.width, plr.height, itemId, maxStack, false, prefix);

                    if (netId < 0)
                        Terraria.Main.item[index].netDefaults(netId);

                    if (prefix > 0)
                        Terraria.Main.item[index].Prefix(prefix);

                    stack -= maxStack; // remove the amount given.
                }

                index = Terraria.Item.NewItem((int)plr.position.X, (int)plr.position.Y, plr.width, plr.height, itemId, stack, false, prefix);

                if (netId < 0)
                    Terraria.Main.item[index].netDefaults(netId);

                if (prefix > 0)
                    Terraria.Main.item[index].Prefix(prefix);

                if (notifyOps)
                    Tools.NotifyAllOps("Giving " + this.Name + " some " + Terraria.Main.item[index].name + " (" + itemId.ToString() + ") [" + sender.SenderName + "]", true);

                return 0;
            }
            return -1;
        }
        #endif
    }
}
