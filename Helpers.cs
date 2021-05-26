using System;
using System.Collections.Generic;
using System.Reflection;
using UnhollowerBaseLib;
using UnityEngine;
using System.Linq;
using Hazel;
using Il2CppSystem.IO;
using static Modpack.Modpack;

namespace Modpack
{
    public static class Helpers
    {
        public static Sprite loadSpriteFromResources(string path, float pixelsPerUnit, bool hat = false)
        {
            try
            {
                var pivot = hat ? new Vector2(0.5f, 0.8f) : new Vector2(0.5f, 0.5f);
                var texture = loadTextureFromResources(path);
                return Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), pivot, pixelsPerUnit);
            }
            catch
            {
                System.Console.WriteLine("Error loading sprite from path: " + path);
            }

            return null;
        }

        public static Texture2D loadTextureFromResources(string path)
        {
            try
            {
                var texture = new Texture2D(2, 2, TextureFormat.ARGB32, true);
                var assembly = Assembly.GetExecutingAssembly();
                var stream = assembly.GetManifestResourceStream(path);
                if (stream == null) return texture;
                var byteTexture = new byte[stream.Length];
                stream.Read(byteTexture, 0, (int) stream.Length);
                LoadImage(texture, byteTexture, false);
                return texture;
            }
            catch
            {
                System.Console.WriteLine("Error loading texture from resources: " + path);
            }

            return null;
        }

        public static Texture2D loadTextureFromDisk(string path)
        {
            try
            {
                if (File.Exists(path))
                {
                    var texture = new Texture2D(2, 2, TextureFormat.ARGB32, true);
                    byte[] byteTexture = File.ReadAllBytes(path);
                    LoadImage(texture, byteTexture, false);
                    return texture;
                }
            }
            catch
            {
                System.Console.WriteLine("Error loading texture from disk: " + path);
            }

            return null;
        }

        internal delegate bool d_LoadImage(IntPtr tex, IntPtr data, bool markNonReadable);

        internal static d_LoadImage iCall_LoadImage;

        private static void LoadImage(Il2CppObjectBase tex, byte[] data, bool markNonReadable)
        {
            iCall_LoadImage ??= IL2CPP.ResolveICall<d_LoadImage>("UnityEngine.ImageConversion::LoadImage");
            var il2cppArray = (Il2CppStructArray<byte>) data;
            iCall_LoadImage.Invoke(tex.Pointer, il2cppArray.Pointer, markNonReadable);
        }

        public static PlayerControl playerById(byte id)
        {
            foreach (var player in PlayerControl.AllPlayerControls)
                if (player.PlayerId == id)
                    return player;
            return null;
        }

        public static Dictionary<byte, PlayerControl> allPlayersById()
        {
            var res = new Dictionary<byte, PlayerControl>();
            foreach (var player in PlayerControl.AllPlayerControls)
                res.Add(player.PlayerId, player);
            return res;
        }

        public static void setSkinWithAnim(PlayerPhysics playerPhysics, uint SkinId)
        {
            SkinData nextSkin = DestroyableSingleton<HatManager>.Instance.AllSkins[(int) SkinId];
            AnimationClip clip = null;
            var spriteAnim = playerPhysics.Skin.animator;
            var anim = spriteAnim.m_animator;
            var skinLayer = playerPhysics.Skin;

            var currentPhysicsAnim = playerPhysics.Animator.GetCurrentAnimation();
            if (currentPhysicsAnim == playerPhysics.RunAnim) clip = nextSkin.RunAnim;
            else if (currentPhysicsAnim == playerPhysics.SpawnAnim) clip = nextSkin.SpawnAnim;
            else if (currentPhysicsAnim == playerPhysics.EnterVentAnim) clip = nextSkin.EnterVentAnim;
            else if (currentPhysicsAnim == playerPhysics.ExitVentAnim) clip = nextSkin.ExitVentAnim;
            else if (currentPhysicsAnim == playerPhysics.IdleAnim) clip = nextSkin.IdleAnim;
            else clip = nextSkin.IdleAnim;

            var progress = playerPhysics.Animator.m_animator.GetCurrentAnimatorStateInfo(0).normalizedTime;
            skinLayer.skin = nextSkin;

            spriteAnim.Play(clip, 1f);
            anim.Play("a", 0, progress % 1);
            anim.Update(0f);
        }

        public static bool handleMurderAttempt(PlayerControl target, bool isMeetingStart = false)
        {
            // Block impostor shielded kill
            if (Medic.shielded != null && Medic.shielded == target)
            {
                var writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId,
                    (byte) CustomRPC.ShieldedMurderAttempt, SendOption.Reliable, -1);
                AmongUsClient.Instance.FinishRpcImmediately(writer);
                RPCProcedure.shieldedMurderAttempt();

                return false;
            }
            // Block impostor not fully grown child kill

            if (Child.child != null && target == Child.child && !Child.isGrownUp())
            {
                return false;
            }

            // Block Time Master with time shield kill
            if (!TimeMaster.shieldActive || TimeMaster.timeMaster == null || TimeMaster.timeMaster != target)
                return true;
            {
                if (isMeetingStart) return false;
                // Only rewind the attempt was not called because a meeting startet 
                var writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId,
                    (byte) CustomRPC.TimeMasterRewindTime, SendOption.Reliable, -1);
                AmongUsClient.Instance.FinishRpcImmediately(writer);
                RPCProcedure.timeMasterRewindTime();

                return false;
            }
        }


        public static void refreshRoleDescription(PlayerControl player)
        {
            if (player == null) return;

            var infos = RoleInfo.getRoleInfoForPlayer(player);

            var toRemove = new List<PlayerTask>();
            foreach (var t in player.myTasks)
            {
                var textTask = t.gameObject.GetComponent<ImportantTextTask>();
                if (textTask == null) continue;
                var info = infos.FirstOrDefault(x => textTask.Text.StartsWith(x.name));
                if (info != null)
                    infos.Remove(
                        info); // TextTask for this RoleInfo does not have to be added, as it already exists
                else
                    toRemove.Add(t); // TextTask does not have a corresponding RoleInfo and will hence be deleted
            }

            foreach (var t in toRemove)
            {
                t.OnRemove();
                player.myTasks.Remove(t);
                UnityEngine.Object.Destroy(t.gameObject);
            }

            // Add TextTask for remaining RoleInfos
            foreach (var roleInfo in infos)
            {
                var task = new GameObject("RoleTask").AddComponent<ImportantTextTask>();
                task.transform.SetParent(player.transform, false);

                if (roleInfo.name == "Jackal")
                {
                    var getSidekickText = Jackal.canCreateSidekick ? " and recruit a Sidekick" : "";
                    task.Text = cs(roleInfo.color, $"{roleInfo.name}: Kill everyone{getSidekickText}");
                }
                else
                {
                    task.Text = cs(roleInfo.color, $"{roleInfo.name}: {roleInfo.shortDescription}");
                }

                player.myTasks.Insert(0, task);
            }
        }

        public static bool isLighterColor(int colorId)
        {
            return CustomColors.lighterColors.Contains(colorId);
        }

        public static bool isCustomServer()
        {
            if (DestroyableSingleton<ServerManager>.Instance == null) return false;
            var n = DestroyableSingleton<ServerManager>.Instance.CurrentRegion.TranslateName;
            return n != StringNames.ServerNA && n != StringNames.ServerEU && n != StringNames.ServerAS;
        }

        public static bool hasFakeTasks(this PlayerControl player)
        {
            return player == Jester.jester || player == Jackal.jackal || player == Sidekick.sidekick ||
                   player == Arsonist.arsonist;
        }

        public static void clearAllTasks(this PlayerControl player)
        {
            if (player == null) return;
            for (var i = 0; i < player.myTasks.Count; i++)
            {
                PlayerTask playerTask = player.myTasks[i];
                playerTask.OnRemove();
                UnityEngine.Object.Destroy(playerTask.gameObject);
            }

            player.myTasks.Clear();

            if (player.Data is {Tasks: { }})
                player.Data.Tasks.Clear();
        }

        public static void setSemiTransparent(this PoolablePlayer player, bool value)
        {
            var alpha = value ? 0.25f : 1f;
            foreach (var r in player.gameObject.GetComponentsInChildren<SpriteRenderer>())
            {
                var color = r.color;
                r.color = new Color(color.r, color.g, color.b, alpha);
            }

            player.NameText.color = new Color(player.NameText.color.r, player.NameText.color.g, player.NameText.color.b,
                alpha);
        }

        public static string cs(Color c, string s)
        {
            return $"<color=#{ToByte(c.r):X2}{ToByte(c.g):X2}{ToByte(c.b):X2}{ToByte(c.a):X2}>{s}</color>";
        }

        private static byte ToByte(float f)
        {
            f = Mathf.Clamp01(f);
            return (byte) (f * 255);
        }
    }
}