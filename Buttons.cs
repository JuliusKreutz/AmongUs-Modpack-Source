using HarmonyLib;
using Hazel;
using System;
using UnityEngine;
using static Modpack.Modpack;
using Object = UnityEngine.Object;

namespace Modpack
{
    [HarmonyPatch(typeof(HudManager), nameof(HudManager.Start))]
    internal static class HudManagerStartPatch
    {
        private static CustomButton engineerRepairButton;
        private static CustomButton janitorCleanButton;
        private static CustomButton sheriffKillButton;
        private static CustomButton timeMasterShieldButton;
        private static CustomButton medicShieldButton;
        private static CustomButton shifterShiftButton;
        private static CustomButton morphlingButton;
        private static CustomButton camouflagerButton;
        private static CustomButton hackerButton;
        private static CustomButton trackerButton;
        private static CustomButton vampireKillButton;
        private static CustomButton garlicButton;
        private static CustomButton jackalKillButton;
        private static CustomButton sidekickKillButton;
        private static CustomButton jackalSidekickButton;
        private static CustomButton lighterButton;
        private static CustomButton eraserButton;
        private static CustomButton placeJackInTheBoxButton;
        private static CustomButton lightsOutButton;
        public static CustomButton cleanerCleanButton;
        public static CustomButton warlockCurseButton;
        public static CustomButton securityGuardButton;
        public static CustomButton arsonistButton;
        public static TMPro.TMP_Text securityGuardButtonScrewsText;

        public static void setCustomButtonCooldowns()
        {
            engineerRepairButton.MaxTimer = 0f;
            janitorCleanButton.MaxTimer = Janitor.cooldown;
            sheriffKillButton.MaxTimer = Sheriff.cooldown;
            timeMasterShieldButton.MaxTimer = TimeMaster.cooldown;
            medicShieldButton.MaxTimer = 0f;
            shifterShiftButton.MaxTimer = 0f;
            morphlingButton.MaxTimer = Morphling.cooldown;
            camouflagerButton.MaxTimer = Camouflager.cooldown;
            hackerButton.MaxTimer = Hacker.cooldown;
            vampireKillButton.MaxTimer = Vampire.cooldown;
            trackerButton.MaxTimer = 0f;
            garlicButton.MaxTimer = 0f;
            jackalKillButton.MaxTimer = Jackal.cooldown;
            sidekickKillButton.MaxTimer = Sidekick.cooldown;
            jackalSidekickButton.MaxTimer = Jackal.createSidekickCooldown;
            lighterButton.MaxTimer = Lighter.cooldown;
            eraserButton.MaxTimer = Eraser.cooldown;
            placeJackInTheBoxButton.MaxTimer = Trickster.placeBoxCooldown;
            lightsOutButton.MaxTimer = Trickster.lightsOutCooldown;
            cleanerCleanButton.MaxTimer = Cleaner.cooldown;
            warlockCurseButton.MaxTimer = Warlock.cooldown;
            securityGuardButton.MaxTimer = SecurityGuard.cooldown;
            arsonistButton.MaxTimer = Arsonist.cooldown;

            timeMasterShieldButton.EffectDuration = TimeMaster.shieldDuration;
            hackerButton.EffectDuration = Hacker.duration;
            vampireKillButton.EffectDuration = Vampire.delay;
            lighterButton.EffectDuration = Lighter.duration;
            camouflagerButton.EffectDuration = Camouflager.duration;
            morphlingButton.EffectDuration = Morphling.duration;
            lightsOutButton.EffectDuration = Trickster.lightsOutDuration;
            arsonistButton.EffectDuration = Arsonist.duration;

            // Already set the timer to the max, as the button is enabled during the game and not available at the start
            lightsOutButton.Timer = lightsOutButton.MaxTimer;
        }

        public static void resetTimeMasterButton()
        {
            timeMasterShieldButton.Timer = timeMasterShieldButton.MaxTimer;
            timeMasterShieldButton.isEffectActive = false;
            timeMasterShieldButton.killButtonManager.TimerText.color = Palette.EnabledColor;
        }

        public static void Postfix(HudManager __instance)
        {
            // Engineer Repair
            engineerRepairButton = new CustomButton(
                () =>
                {
                    engineerRepairButton.Timer = 0f;

                    var usedRepairWriter = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId,
                        (byte) CustomRPC.EngineerUsedRepair, SendOption.Reliable, -1);
                    AmongUsClient.Instance.FinishRpcImmediately(usedRepairWriter);
                    RPCProcedure.engineerUsedRepair();

                    foreach (var task in PlayerControl.LocalPlayer.myTasks)
                    {
                        switch (task.TaskType)
                        {
                            case TaskTypes.FixLights:
                            {
                                var writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId,
                                    (byte) CustomRPC.EngineerFixLights, SendOption.Reliable, -1);
                                AmongUsClient.Instance.FinishRpcImmediately(writer);
                                RPCProcedure.engineerFixLights();
                                break;
                            }
                            case TaskTypes.RestoreOxy:
                                ShipStatus.Instance.RpcRepairSystem(SystemTypes.LifeSupp, 0 | 64);
                                ShipStatus.Instance.RpcRepairSystem(SystemTypes.LifeSupp, 1 | 64);
                                break;
                            case TaskTypes.ResetReactor:
                                ShipStatus.Instance.RpcRepairSystem(SystemTypes.Reactor, 16);
                                break;
                            case TaskTypes.ResetSeismic:
                                ShipStatus.Instance.RpcRepairSystem(SystemTypes.Laboratory, 16);
                                break;
                            case TaskTypes.FixComms:
                                ShipStatus.Instance.RpcRepairSystem(SystemTypes.Comms, 16 | 0);
                                ShipStatus.Instance.RpcRepairSystem(SystemTypes.Comms, 16 | 1);
                                break;
                            case TaskTypes.StopCharles:
                                ShipStatus.Instance.RpcRepairSystem(SystemTypes.Reactor, 0 | 16);
                                ShipStatus.Instance.RpcRepairSystem(SystemTypes.Reactor, 1 | 16);
                                break;
                        }
                    }
                },
                () => Engineer.engineer != null && Engineer.engineer == PlayerControl.LocalPlayer &&
                      !PlayerControl.LocalPlayer.Data.IsDead,
                () =>
                {
                    var sabotageActive = false;
                    foreach (var task in PlayerControl.LocalPlayer.myTasks)
                        if (task.TaskType == TaskTypes.FixLights || task.TaskType == TaskTypes.RestoreOxy ||
                            task.TaskType == TaskTypes.ResetReactor || task.TaskType == TaskTypes.ResetSeismic ||
                            task.TaskType == TaskTypes.FixComms || task.TaskType == TaskTypes.StopCharles)
                            sabotageActive = true;
                    return sabotageActive && !Engineer.usedRepair && PlayerControl.LocalPlayer.CanMove;
                },
                () => { },
                Engineer.getButtonSprite(),
                new Vector3(-1.3f, 0, 0),
                __instance,
                KeyCode.Q
            );

            // Janitor Clean
            janitorCleanButton = new CustomButton(
                () =>
                {
                    foreach (var collider2D in Physics2D.OverlapCircleAll(PlayerControl.LocalPlayer.GetTruePosition(),
                        PlayerControl.LocalPlayer.MaxReportDistance, Constants.PlayersOnlyMask))
                    {
                        if (collider2D.tag != "DeadBody") continue;
                        var component = collider2D.GetComponent<DeadBody>();
                        if (!component || component.Reported) continue;
                        var truePosition = PlayerControl.LocalPlayer.GetTruePosition();
                        var truePosition2 = component.TruePosition;
                        if (!(Vector2.Distance(truePosition2, truePosition) <=
                              PlayerControl.LocalPlayer.MaxReportDistance) || !PlayerControl.LocalPlayer.CanMove ||
                            PhysicsHelpers.AnythingBetween(truePosition, truePosition2, Constants.ShipAndObjectsMask,
                                false)) continue;
                        var playerInfo = GameData.Instance.GetPlayerById(component.ParentId);

                        var writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId,
                            (byte) CustomRPC.CleanBody, SendOption.Reliable, -1);
                        writer.Write(playerInfo.PlayerId);
                        AmongUsClient.Instance.FinishRpcImmediately(writer);
                        RPCProcedure.cleanBody(playerInfo.PlayerId);
                        janitorCleanButton.Timer = janitorCleanButton.MaxTimer;

                        break;
                    }
                },
                () => Janitor.janitor != null && Janitor.janitor == PlayerControl.LocalPlayer &&
                      !PlayerControl.LocalPlayer.Data.IsDead,
                () => __instance.ReportButton.renderer.color == Palette.EnabledColor &&
                      PlayerControl.LocalPlayer.CanMove,
                () => { janitorCleanButton.Timer = janitorCleanButton.MaxTimer; },
                Janitor.getButtonSprite(),
                new Vector3(-1.3f, 0, 0),
                __instance,
                KeyCode.Q
            );

            // Sheriff Kill
            sheriffKillButton = new CustomButton(
                () =>
                {
                    if (Medic.shielded != null && Medic.shielded == Sheriff.currentTarget)
                    {
                        var attemptWriter = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId,
                            (byte) CustomRPC.ShieldedMurderAttempt, SendOption.Reliable, -1);
                        AmongUsClient.Instance.FinishRpcImmediately(attemptWriter);
                        RPCProcedure.shieldedMurderAttempt();
                        return;
                    }

                    byte targetId = 0;
                    if (Sheriff.currentTarget.Data.IsImpostor &&
                        (Sheriff.currentTarget != Child.child || Child.isGrownUp()) ||
                        Sheriff.spyCanDieToSheriff && Spy.spy == Sheriff.currentTarget ||
                        Sheriff.canKillNeutrals && (Arsonist.arsonist == Sheriff.currentTarget ||
                                                    Jester.jester == Sheriff.currentTarget ||
                                                    Jackal.jackal == Sheriff.currentTarget ||
                                                    Sidekick.sidekick == Sheriff.currentTarget))
                    {
                        targetId = Sheriff.currentTarget.PlayerId;
                    }
                    else
                    {
                        targetId = PlayerControl.LocalPlayer.PlayerId;
                    }

                    var killWriter = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId,
                        (byte) CustomRPC.SheriffKill, SendOption.Reliable, -1);
                    killWriter.Write(targetId);
                    AmongUsClient.Instance.FinishRpcImmediately(killWriter);
                    RPCProcedure.sheriffKill(targetId);

                    sheriffKillButton.Timer = sheriffKillButton.MaxTimer;
                    Sheriff.currentTarget = null;
                },
                () => Sheriff.sheriff != null && Sheriff.sheriff == PlayerControl.LocalPlayer &&
                      !PlayerControl.LocalPlayer.Data.IsDead,
                () => Sheriff.currentTarget && PlayerControl.LocalPlayer.CanMove,
                () => { sheriffKillButton.Timer = sheriffKillButton.MaxTimer; },
                __instance.KillButton.renderer.sprite,
                new Vector3(-1.3f, 0, 0),
                __instance,
                KeyCode.Q
            );

            // Time Master Rewind Time
            timeMasterShieldButton = new CustomButton(
                () =>
                {
                    var writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId,
                        (byte) CustomRPC.TimeMasterShield, SendOption.Reliable, -1);
                    AmongUsClient.Instance.FinishRpcImmediately(writer);
                    RPCProcedure.timeMasterShield();
                },
                () => TimeMaster.timeMaster != null && TimeMaster.timeMaster == PlayerControl.LocalPlayer &&
                      !PlayerControl.LocalPlayer.Data.IsDead,
                () => PlayerControl.LocalPlayer.CanMove,
                () =>
                {
                    timeMasterShieldButton.Timer = timeMasterShieldButton.MaxTimer;
                    timeMasterShieldButton.isEffectActive = false;
                    timeMasterShieldButton.killButtonManager.TimerText.color = Palette.EnabledColor;
                },
                TimeMaster.getButtonSprite(),
                new Vector3(-1.3f, 0, 0),
                __instance,
                KeyCode.Q,
                true,
                TimeMaster.shieldDuration,
                () => { timeMasterShieldButton.Timer = timeMasterShieldButton.MaxTimer; }
            );

            // Medic Shield
            medicShieldButton = new CustomButton(
                () =>
                {
                    medicShieldButton.Timer = 0f;

                    var writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId,
                        (byte) CustomRPC.MedicSetShielded, SendOption.Reliable, -1);
                    writer.Write(Medic.currentTarget.PlayerId);
                    AmongUsClient.Instance.FinishRpcImmediately(writer);
                    RPCProcedure.medicSetShielded(Medic.currentTarget.PlayerId);
                },
                () => Medic.medic != null && Medic.medic == PlayerControl.LocalPlayer &&
                      !PlayerControl.LocalPlayer.Data.IsDead,
                () => !Medic.usedShield && Medic.currentTarget && PlayerControl.LocalPlayer.CanMove,
                () => { },
                Medic.getButtonSprite(),
                new Vector3(-1.3f, 0, 0),
                __instance,
                KeyCode.Q
            );


            // Shifter shift
            shifterShiftButton = new CustomButton(
                () =>
                {
                    var writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId,
                        (byte) CustomRPC.SetFutureShifted, SendOption.Reliable, -1);
                    writer.Write(Shifter.currentTarget.PlayerId);
                    AmongUsClient.Instance.FinishRpcImmediately(writer);
                    RPCProcedure.setFutureShifted(Shifter.currentTarget.PlayerId);
                },
                () => Shifter.shifter != null && Shifter.shifter == PlayerControl.LocalPlayer &&
                      !PlayerControl.LocalPlayer.Data.IsDead,
                () => Shifter.currentTarget && Shifter.futureShift == null && PlayerControl.LocalPlayer.CanMove,
                () => { },
                Shifter.getButtonSprite(),
                new Vector3(-1.3f, 0, 0),
                __instance,
                KeyCode.Q
            );

            // Morphling morph
            morphlingButton = new CustomButton(
                () =>
                {
                    if (Morphling.sampledTarget != null)
                    {
                        var writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId,
                            (byte) CustomRPC.MorphlingMorph, SendOption.Reliable, -1);
                        writer.Write(Morphling.sampledTarget.PlayerId);
                        AmongUsClient.Instance.FinishRpcImmediately(writer);
                        RPCProcedure.morphlingMorph(Morphling.sampledTarget.PlayerId);
                        Morphling.sampledTarget = null;
                        morphlingButton.EffectDuration = Morphling.duration;
                    }
                    else if (Morphling.currentTarget != null)
                    {
                        Morphling.sampledTarget = Morphling.currentTarget;
                        morphlingButton.Sprite = Morphling.getMorphSprite();
                        morphlingButton.EffectDuration = 1f;
                    }
                },
                () => Morphling.morphling != null && Morphling.morphling == PlayerControl.LocalPlayer &&
                      !PlayerControl.LocalPlayer.Data.IsDead,
                () => (Morphling.currentTarget || Morphling.sampledTarget) && PlayerControl.LocalPlayer.CanMove,
                () =>
                {
                    morphlingButton.Timer = morphlingButton.MaxTimer;
                    morphlingButton.Sprite = Morphling.getSampleSprite();
                    morphlingButton.isEffectActive = false;
                    morphlingButton.killButtonManager.TimerText.color = Palette.EnabledColor;
                    Morphling.sampledTarget = null;
                },
                Morphling.getSampleSprite(),
                new Vector3(-1.3f, 1.3f, 0f),
                __instance,
                KeyCode.F,
                true,
                Morphling.duration,
                () =>
                {
                    if (Morphling.sampledTarget != null) return;
                    morphlingButton.Timer = morphlingButton.MaxTimer;
                    morphlingButton.Sprite = Morphling.getSampleSprite();
                }
            );

            // Camouflager camouflage
            camouflagerButton = new CustomButton(
                () =>
                {
                    var writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId,
                        (byte) CustomRPC.CamouflagerCamouflage, SendOption.Reliable, -1);
                    AmongUsClient.Instance.FinishRpcImmediately(writer);
                    RPCProcedure.camouflagerCamouflage();
                },
                () => Camouflager.camouflager != null && Camouflager.camouflager == PlayerControl.LocalPlayer &&
                      !PlayerControl.LocalPlayer.Data.IsDead,
                () => PlayerControl.LocalPlayer.CanMove,
                () =>
                {
                    camouflagerButton.Timer = camouflagerButton.MaxTimer;
                    camouflagerButton.isEffectActive = false;
                    camouflagerButton.killButtonManager.TimerText.color = Palette.EnabledColor;
                },
                Camouflager.getButtonSprite(),
                new Vector3(-1.3f, 1.3f, 0f),
                __instance,
                KeyCode.F,
                true,
                Camouflager.duration,
                () => { camouflagerButton.Timer = camouflagerButton.MaxTimer; }
            );

            // Hacker button
            hackerButton = new CustomButton(
                () => { Hacker.hackerTimer = Hacker.duration; },
                () => Hacker.hacker != null && Hacker.hacker == PlayerControl.LocalPlayer &&
                      !PlayerControl.LocalPlayer.Data.IsDead,
                () => PlayerControl.LocalPlayer.CanMove,
                () =>
                {
                    hackerButton.Timer = hackerButton.MaxTimer;
                    hackerButton.isEffectActive = false;
                    hackerButton.killButtonManager.TimerText.color = Palette.EnabledColor;
                },
                Hacker.getButtonSprite(),
                new Vector3(-1.3f, 0, 0),
                __instance,
                KeyCode.Q,
                true,
                0f,
                () => { hackerButton.Timer = hackerButton.MaxTimer; }
            );

            // Tracker button
            trackerButton = new CustomButton(
                () =>
                {
                    var writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId,
                        (byte) CustomRPC.TrackerUsedTracker, SendOption.Reliable, -1);
                    writer.Write(Tracker.currentTarget.PlayerId);
                    AmongUsClient.Instance.FinishRpcImmediately(writer);
                    RPCProcedure.trackerUsedTracker(Tracker.currentTarget.PlayerId);
                },
                () => Tracker.tracker != null && Tracker.tracker == PlayerControl.LocalPlayer &&
                      !PlayerControl.LocalPlayer.Data.IsDead,
                () => PlayerControl.LocalPlayer.CanMove && Tracker.currentTarget != null && !Tracker.usedTracker,
                () => { },
                Tracker.getButtonSprite(),
                new Vector3(-1.3f, 0, 0),
                __instance,
                KeyCode.Q
            );

            vampireKillButton = new CustomButton(
                () =>
                {
                    if (Helpers.handleMurderAttempt(Vampire.currentTarget))
                    {
                        if (Vampire.targetNearGarlic)
                        {
                            var writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId,
                                (byte) CustomRPC.UncheckedMurderPlayer, SendOption.Reliable, -1);
                            writer.Write(Vampire.vampire.PlayerId);
                            writer.Write(Vampire.currentTarget.PlayerId);
                            AmongUsClient.Instance.FinishRpcImmediately(writer);
                            RPCProcedure.uncheckedMurderPlayer(Vampire.vampire.PlayerId,
                                Vampire.currentTarget.PlayerId);

                            vampireKillButton.HasEffect = false; // Block effect on this click
                            vampireKillButton.Timer = vampireKillButton.MaxTimer;
                        }
                        else
                        {
                            Vampire.bitten = Vampire.currentTarget;
                            // Notify players about bitten
                            var writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId,
                                (byte) CustomRPC.VampireSetBitten, SendOption.Reliable, -1);
                            writer.Write(Vampire.bitten.PlayerId);
                            writer.Write(0);
                            AmongUsClient.Instance.FinishRpcImmediately(writer);
                            RPCProcedure.vampireSetBitten(Vampire.bitten.PlayerId, 0);

                            HudManager.Instance.StartCoroutine(Effects.Lerp(Vampire.delay, new Action<float>((p) =>
                            {
                                // Delayed action
                                if (p != 1f) return;
                                if (Vampire.bitten != null && !Vampire.bitten.Data.IsDead &&
                                    Helpers.handleMurderAttempt(Vampire.bitten))
                                {
                                    // Perform kill
                                    var killWriter = AmongUsClient.Instance.StartRpcImmediately(
                                        PlayerControl.LocalPlayer.NetId, (byte) CustomRPC.VampireTryKill,
                                        SendOption.Reliable, -1);
                                    AmongUsClient.Instance.FinishRpcImmediately(killWriter);
                                    RPCProcedure.vampireTryKill();
                                }
                                else
                                {
                                    // Notify players about clearing bitten
                                    var rpcWriter = AmongUsClient.Instance.StartRpcImmediately(
                                        PlayerControl.LocalPlayer.NetId, (byte) CustomRPC.VampireSetBitten,
                                        SendOption.Reliable, -1);
                                    rpcWriter.Write(byte.MaxValue);
                                    rpcWriter.Write(byte.MaxValue);
                                    AmongUsClient.Instance.FinishRpcImmediately(rpcWriter);
                                    RPCProcedure.vampireSetBitten(byte.MaxValue, byte.MaxValue);
                                }
                            })));

                            vampireKillButton.HasEffect = true; // Trigger effect on this click
                        }
                    }
                    else
                    {
                        vampireKillButton.HasEffect = false; // Block effect if no action was fired
                    }
                },
                () => Vampire.vampire != null && Vampire.vampire == PlayerControl.LocalPlayer &&
                      !PlayerControl.LocalPlayer.Data.IsDead,
                () =>
                {
                    if (Vampire.targetNearGarlic && Vampire.canKillNearGarlics)
                        vampireKillButton.killButtonManager.renderer.sprite = __instance.KillButton.renderer.sprite;
                    else
                        vampireKillButton.killButtonManager.renderer.sprite = Vampire.getButtonSprite();
                    return Vampire.currentTarget != null && PlayerControl.LocalPlayer.CanMove &&
                           (!Vampire.targetNearGarlic || Vampire.canKillNearGarlics);
                },
                () =>
                {
                    vampireKillButton.Timer = vampireKillButton.MaxTimer;
                    vampireKillButton.isEffectActive = false;
                    vampireKillButton.killButtonManager.TimerText.color = Palette.EnabledColor;
                },
                Vampire.getButtonSprite(),
                new Vector3(-1.3f, 0, 0),
                __instance,
                KeyCode.Q,
                false,
                0f,
                () => { vampireKillButton.Timer = vampireKillButton.MaxTimer; }
            );

            garlicButton = new CustomButton(
                () =>
                {
                    Vampire.localPlacedGarlic = true;
                    var pos = PlayerControl.LocalPlayer.transform.position;
                    var buff = new byte[sizeof(float) * 2];
                    Buffer.BlockCopy(BitConverter.GetBytes(pos.x), 0, buff, 0 * sizeof(float), sizeof(float));
                    Buffer.BlockCopy(BitConverter.GetBytes(pos.y), 0, buff, 1 * sizeof(float), sizeof(float));

                    var writer = AmongUsClient.Instance.StartRpc(PlayerControl.LocalPlayer.NetId,
                        (byte) CustomRPC.PlaceGarlic, SendOption.Reliable);
                    writer.WriteBytesAndSize(buff);
                    writer.EndMessage();
                    RPCProcedure.placeGarlic(buff);
                },
                () => !Vampire.localPlacedGarlic && !PlayerControl.LocalPlayer.Data.IsDead && Vampire.garlicsActive,
                () => PlayerControl.LocalPlayer.CanMove && !Vampire.localPlacedGarlic,
                () => { },
                Vampire.getGarlicButtonSprite(),
                Vector3.zero,
                __instance,
                null,
                true
            );


            // Jackal Sidekick Button
            jackalSidekickButton = new CustomButton(
                () =>
                {
                    var writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId,
                        (byte) CustomRPC.JackalCreatesSidekick, SendOption.Reliable, -1);
                    writer.Write(Jackal.currentTarget.PlayerId);
                    AmongUsClient.Instance.FinishRpcImmediately(writer);
                    RPCProcedure.jackalCreatesSidekick(Jackal.currentTarget.PlayerId);
                },
                () => Jackal.canCreateSidekick && Sidekick.sidekick == null && Jackal.fakeSidekick == null &&
                      Jackal.jackal != null && Jackal.jackal == PlayerControl.LocalPlayer &&
                      !PlayerControl.LocalPlayer.Data.IsDead,
                () => Sidekick.sidekick == null && Jackal.fakeSidekick == null && Jackal.currentTarget != null &&
                      PlayerControl.LocalPlayer.CanMove,
                () => { jackalSidekickButton.Timer = jackalSidekickButton.MaxTimer; },
                Jackal.getSidekickButtonSprite(),
                new Vector3(-1.3f, 1.3f, 0f),
                __instance,
                KeyCode.F
            );

            // Jackal Kill
            var sprite = __instance.KillButton.renderer.sprite;
            jackalKillButton = new CustomButton(
                () =>
                {
                    if (!Helpers.handleMurderAttempt(Jackal.currentTarget)) return;
                    var targetId = Jackal.currentTarget.PlayerId;
                    var killWriter = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId,
                        (byte) CustomRPC.JackalKill, SendOption.Reliable, -1);
                    killWriter.Write(targetId);
                    AmongUsClient.Instance.FinishRpcImmediately(killWriter);
                    RPCProcedure.jackalKill(targetId);
                    jackalKillButton.Timer = jackalKillButton.MaxTimer;
                    Jackal.currentTarget = null;
                },
                () => Jackal.jackal != null && Jackal.jackal == PlayerControl.LocalPlayer &&
                      !PlayerControl.LocalPlayer.Data.IsDead,
                () => Jackal.currentTarget && PlayerControl.LocalPlayer.CanMove,
                () => { jackalKillButton.Timer = jackalKillButton.MaxTimer; },
                sprite,
                new Vector3(-1.3f, 0, 0),
                __instance,
                KeyCode.Q
            );

            // Sidekick Kill
            sidekickKillButton = new CustomButton(
                () =>
                {
                    if (!Helpers.handleMurderAttempt(Sidekick.currentTarget)) return;
                    var targetId = Sidekick.currentTarget.PlayerId;
                    var killWriter = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId,
                        (byte) CustomRPC.SidekickKill, SendOption.Reliable, -1);
                    killWriter.Write(targetId);
                    AmongUsClient.Instance.FinishRpcImmediately(killWriter);
                    RPCProcedure.sidekickKill(targetId);

                    sidekickKillButton.Timer = sidekickKillButton.MaxTimer;
                    Sidekick.currentTarget = null;
                },
                () => Sidekick.canKill && Sidekick.sidekick != null && Sidekick.sidekick == PlayerControl.LocalPlayer &&
                      !PlayerControl.LocalPlayer.Data.IsDead,
                () => Sidekick.currentTarget && PlayerControl.LocalPlayer.CanMove,
                () => { sidekickKillButton.Timer = sidekickKillButton.MaxTimer; },
                sprite,
                new Vector3(-1.3f, 0, 0),
                __instance,
                KeyCode.Q
            );

            // Lighter light
            lighterButton = new CustomButton(
                () => { Lighter.lighterTimer = Lighter.duration; },
                () => Lighter.lighter != null && Lighter.lighter == PlayerControl.LocalPlayer &&
                      !PlayerControl.LocalPlayer.Data.IsDead,
                () => PlayerControl.LocalPlayer.CanMove,
                () =>
                {
                    lighterButton.Timer = lighterButton.MaxTimer;
                    lighterButton.isEffectActive = false;
                    lighterButton.killButtonManager.TimerText.color = Palette.EnabledColor;
                },
                Lighter.getButtonSprite(),
                new Vector3(-1.3f, 0f, 0f),
                __instance,
                KeyCode.Q,
                true,
                Lighter.duration,
                () => { lighterButton.Timer = lighterButton.MaxTimer; }
            );

            // Eraser erase button
            eraserButton = new CustomButton(
                () =>
                {
                    eraserButton.MaxTimer += 10;
                    eraserButton.Timer = eraserButton.MaxTimer;

                    var writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId,
                        (byte) CustomRPC.SetFutureErased, SendOption.Reliable, -1);
                    writer.Write(Eraser.currentTarget.PlayerId);
                    AmongUsClient.Instance.FinishRpcImmediately(writer);
                    RPCProcedure.setFutureErased(Eraser.currentTarget.PlayerId);
                },
                () => Eraser.eraser != null && Eraser.eraser == PlayerControl.LocalPlayer &&
                      !PlayerControl.LocalPlayer.Data.IsDead,
                () => PlayerControl.LocalPlayer.CanMove && Eraser.currentTarget != null,
                () => { eraserButton.Timer = eraserButton.MaxTimer; },
                Eraser.getButtonSprite(),
                new Vector3(-1.3f, 1.3f, 0f),
                __instance,
                KeyCode.F
            );

            placeJackInTheBoxButton = new CustomButton(
                () =>
                {
                    placeJackInTheBoxButton.Timer = placeJackInTheBoxButton.MaxTimer;

                    var pos = PlayerControl.LocalPlayer.transform.position;
                    var buff = new byte[sizeof(float) * 2];
                    Buffer.BlockCopy(BitConverter.GetBytes(pos.x), 0, buff, 0 * sizeof(float), sizeof(float));
                    Buffer.BlockCopy(BitConverter.GetBytes(pos.y), 0, buff, 1 * sizeof(float), sizeof(float));

                    var writer = AmongUsClient.Instance.StartRpc(PlayerControl.LocalPlayer.NetId,
                        (byte) CustomRPC.PlaceJackInTheBox, SendOption.Reliable);
                    writer.WriteBytesAndSize(buff);
                    writer.EndMessage();
                    RPCProcedure.placeJackInTheBox(buff);
                },
                () => Trickster.trickster != null && Trickster.trickster == PlayerControl.LocalPlayer &&
                      !PlayerControl.LocalPlayer.Data.IsDead && !JackInTheBox.hasJackInTheBoxLimitReached(),
                () => PlayerControl.LocalPlayer.CanMove && !JackInTheBox.hasJackInTheBoxLimitReached(),
                () => { placeJackInTheBoxButton.Timer = placeJackInTheBoxButton.MaxTimer; },
                Trickster.getPlaceBoxButtonSprite(),
                new Vector3(-1.3f, 1.3f, 0f),
                __instance,
                KeyCode.F
            );

            lightsOutButton = new CustomButton(
                () =>
                {
                    var writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId,
                        (byte) CustomRPC.LightsOut, SendOption.Reliable, -1);
                    AmongUsClient.Instance.FinishRpcImmediately(writer);
                    RPCProcedure.lightsOut();
                },
                () => Trickster.trickster != null && Trickster.trickster == PlayerControl.LocalPlayer &&
                      !PlayerControl.LocalPlayer.Data.IsDead && JackInTheBox.hasJackInTheBoxLimitReached() &&
                      JackInTheBox.boxesConvertedToVents,
                () => PlayerControl.LocalPlayer.CanMove && JackInTheBox.hasJackInTheBoxLimitReached() &&
                      JackInTheBox.boxesConvertedToVents,
                () =>
                {
                    lightsOutButton.Timer = lightsOutButton.MaxTimer;
                    lightsOutButton.isEffectActive = false;
                    lightsOutButton.killButtonManager.TimerText.color = Palette.EnabledColor;
                },
                Trickster.getLightsOutButtonSprite(),
                new Vector3(-1.3f, 1.3f, 0f),
                __instance,
                KeyCode.F,
                true,
                Trickster.lightsOutDuration,
                () => { lightsOutButton.Timer = lightsOutButton.MaxTimer; }
            );
            // Cleaner Clean
            cleanerCleanButton = new CustomButton(
                () =>
                {
                    foreach (var collider2D in Physics2D.OverlapCircleAll(PlayerControl.LocalPlayer.GetTruePosition(),
                        PlayerControl.LocalPlayer.MaxReportDistance, Constants.PlayersOnlyMask))
                    {
                        if (collider2D.tag != "DeadBody") continue;
                        var component = collider2D.GetComponent<DeadBody>();
                        if (!component || component.Reported) continue;
                        var truePosition = PlayerControl.LocalPlayer.GetTruePosition();
                        var truePosition2 = component.TruePosition;
                        if (!(Vector2.Distance(truePosition2, truePosition) <=
                              PlayerControl.LocalPlayer.MaxReportDistance) || !PlayerControl.LocalPlayer.CanMove ||
                            PhysicsHelpers.AnythingBetween(truePosition, truePosition2, Constants.ShipAndObjectsMask,
                                false)) continue;
                        var playerInfo = GameData.Instance.GetPlayerById(component.ParentId);

                        var writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId,
                            (byte) CustomRPC.CleanBody, SendOption.Reliable, -1);
                        writer.Write(playerInfo.PlayerId);
                        AmongUsClient.Instance.FinishRpcImmediately(writer);
                        RPCProcedure.cleanBody(playerInfo.PlayerId);

                        Cleaner.cleaner.killTimer = cleanerCleanButton.Timer = cleanerCleanButton.MaxTimer;
                        break;
                    }
                },
                () => Cleaner.cleaner != null && Cleaner.cleaner == PlayerControl.LocalPlayer &&
                      !PlayerControl.LocalPlayer.Data.IsDead,
                () => __instance.ReportButton.renderer.color == Palette.EnabledColor &&
                      PlayerControl.LocalPlayer.CanMove,
                () => { cleanerCleanButton.Timer = cleanerCleanButton.MaxTimer; },
                Cleaner.getButtonSprite(),
                new Vector3(-1.3f, 1.3f, 0f),
                __instance,
                KeyCode.F
            );

            // Warlock curse
            warlockCurseButton = new CustomButton(
                () =>
                {
                    if (Warlock.curseVictim == null)
                    {
                        // Apply Curse
                        Warlock.curseVictim = Warlock.currentTarget;
                        warlockCurseButton.Sprite = Warlock.getCurseKillButtonSprite();
                        warlockCurseButton.Timer = 1f;
                    }
                    else if (Warlock.curseVictim != null && Warlock.curseVictimTarget != null &&
                             Helpers.handleMurderAttempt(Warlock.curseVictimTarget))
                    {
                        // Curse Kill
                        Warlock.curseKillTarget = Warlock.curseVictimTarget;

                        var writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId,
                            (byte) CustomRPC.WarlockCurseKill, SendOption.Reliable, -1);
                        writer.Write(Warlock.curseKillTarget.PlayerId);
                        AmongUsClient.Instance.FinishRpcImmediately(writer);
                        RPCProcedure.warlockCurseKill(Warlock.curseKillTarget.PlayerId);

                        Warlock.curseVictim = null;
                        Warlock.curseVictimTarget = null;
                        warlockCurseButton.Sprite = Warlock.getCurseButtonSprite();
                        Warlock.warlock.killTimer = warlockCurseButton.Timer = warlockCurseButton.MaxTimer;

                        if (!(Warlock.rootTime > 0)) return;
                        PlayerControl.LocalPlayer.moveable = false;
                        PlayerControl.LocalPlayer.NetTransform
                            .Halt(); // Stop current movement so the warlock is not just running straight into the next object
                        HudManager.Instance.StartCoroutine(Effects.Lerp(Warlock.rootTime, new Action<float>((p) =>
                        {
                            // Delayed action
                            if (p == 1f)
                            {
                                PlayerControl.LocalPlayer.moveable = true;
                            }
                        })));
                    }
                },
                () => Warlock.warlock != null && Warlock.warlock == PlayerControl.LocalPlayer &&
                      !PlayerControl.LocalPlayer.Data.IsDead,
                () => (Warlock.curseVictim == null && Warlock.currentTarget != null ||
                       Warlock.curseVictim != null && Warlock.curseVictimTarget != null) &&
                      PlayerControl.LocalPlayer.CanMove,
                () =>
                {
                    warlockCurseButton.Timer = warlockCurseButton.MaxTimer;
                    warlockCurseButton.Sprite = Warlock.getCurseButtonSprite();
                    Warlock.curseVictim = null;
                    Warlock.curseVictimTarget = null;
                },
                Warlock.getCurseButtonSprite(),
                new Vector3(-1.3f, 1.3f, 0f),
                __instance,
                KeyCode.F
            );

            // Security Guard button
            securityGuardButton = new CustomButton(
                () =>
                {
                    if (SecurityGuard.ventTarget != null)
                    {
                        // Seal vent
                        var writer = AmongUsClient.Instance.StartRpc(PlayerControl.LocalPlayer.NetId,
                            (byte) CustomRPC.SealVent, SendOption.Reliable);
                        writer.WritePacked(SecurityGuard.ventTarget.Id);
                        writer.EndMessage();
                        RPCProcedure.sealVent(SecurityGuard.ventTarget.Id);
                        SecurityGuard.ventTarget = null;
                    }
                    else if (PlayerControl.GameOptions.MapId != 1)
                    {
                        // Place camera if there's no vent and it's not MiraHQ
                        var pos = PlayerControl.LocalPlayer.transform.position;
                        var buff = new byte[sizeof(float) * 2];
                        Buffer.BlockCopy(BitConverter.GetBytes(pos.x), 0, buff, 0 * sizeof(float), sizeof(float));
                        Buffer.BlockCopy(BitConverter.GetBytes(pos.y), 0, buff, 1 * sizeof(float), sizeof(float));

                        var writer = AmongUsClient.Instance.StartRpc(PlayerControl.LocalPlayer.NetId,
                            (byte) CustomRPC.PlaceCamera, SendOption.Reliable);
                        writer.WriteBytesAndSize(buff);
                        writer.EndMessage();
                        RPCProcedure.placeCamera(buff);
                    }

                    securityGuardButton.Timer = securityGuardButton.MaxTimer;
                },
                () => SecurityGuard.securityGuard != null && SecurityGuard.securityGuard == PlayerControl.LocalPlayer &&
                      !PlayerControl.LocalPlayer.Data.IsDead && SecurityGuard.remainingScrews >=
                      Mathf.Min(SecurityGuard.ventPrice, SecurityGuard.camPrice),
                () =>
                {
                    securityGuardButton.killButtonManager.renderer.sprite =
                        SecurityGuard.ventTarget == null && PlayerControl.GameOptions.MapId != 1
                            ? SecurityGuard.getPlaceCameraButtonSprite()
                            : SecurityGuard.getCloseVentButtonSprite();
                    if (securityGuardButtonScrewsText != null)
                        securityGuardButtonScrewsText.text =
                            $"{SecurityGuard.remainingScrews}/{SecurityGuard.totalScrews}";

                    if (SecurityGuard.ventTarget != null)
                        return SecurityGuard.remainingScrews >= SecurityGuard.ventPrice &&
                               PlayerControl.LocalPlayer.CanMove;
                    return PlayerControl.GameOptions.MapId != 1 &&
                           SecurityGuard.remainingScrews >= SecurityGuard.camPrice && PlayerControl.LocalPlayer.CanMove;
                },
                () => { securityGuardButton.Timer = securityGuardButton.MaxTimer; },
                SecurityGuard.getPlaceCameraButtonSprite(),
                new Vector3(-1.3f, 0f, 0f),
                __instance,
                KeyCode.Q
            );

            // Security Guard button screws counter
            securityGuardButtonScrewsText = Object.Instantiate(securityGuardButton.killButtonManager.TimerText,
                securityGuardButton.killButtonManager.TimerText.transform.parent);
            securityGuardButtonScrewsText.text = "";
            securityGuardButtonScrewsText.enableWordWrapping = false;
            securityGuardButtonScrewsText.transform.localScale = Vector3.one * 0.5f;
            securityGuardButtonScrewsText.transform.localPosition += new Vector3(-0.05f, 0.7f, 0);

            // Arsonist button
            arsonistButton = new CustomButton(
                () =>
                {
                    bool dousedEveryoneAlive = Arsonist.dousedEveryoneAlive();
                    if (dousedEveryoneAlive)
                    {
                        var winWriter = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId,
                            (byte) CustomRPC.ArsonistWin, SendOption.Reliable, -1);
                        AmongUsClient.Instance.FinishRpcImmediately(winWriter);
                        RPCProcedure.arsonistWin();
                        arsonistButton.HasEffect = false;
                    }
                    else if (Arsonist.currentTarget != null)
                    {
                        Arsonist.douseTarget = Arsonist.currentTarget;
                        arsonistButton.HasEffect = true;
                    }
                },
                () => Arsonist.arsonist != null && Arsonist.arsonist == PlayerControl.LocalPlayer &&
                      !PlayerControl.LocalPlayer.Data.IsDead,
                () =>
                {
                    bool dousedEveryoneAlive = Arsonist.dousedEveryoneAlive();
                    if (dousedEveryoneAlive)
                        arsonistButton.killButtonManager.renderer.sprite = Arsonist.getIgniteSprite();

                    if (!arsonistButton.isEffectActive || Arsonist.douseTarget == Arsonist.currentTarget)
                        return PlayerControl.LocalPlayer.CanMove &&
                               (dousedEveryoneAlive || Arsonist.currentTarget != null);
                    Arsonist.douseTarget = null;
                    arsonistButton.Timer = 0f;
                    arsonistButton.isEffectActive = false;

                    return PlayerControl.LocalPlayer.CanMove && (dousedEveryoneAlive || Arsonist.currentTarget != null);
                },
                () =>
                {
                    arsonistButton.Timer = arsonistButton.MaxTimer;
                    arsonistButton.isEffectActive = false;
                    Arsonist.douseTarget = null;
                },
                Arsonist.getDouseSprite(),
                new Vector3(-1.3f, 0f, 0f),
                __instance,
                KeyCode.Q,
                true,
                Arsonist.duration,
                () =>
                {
                    if (Arsonist.douseTarget != null) Arsonist.dousedPlayers.Add(Arsonist.douseTarget);
                    Arsonist.douseTarget = null;
                    arsonistButton.Timer = Arsonist.dousedEveryoneAlive() ? 0 : arsonistButton.MaxTimer;

                    foreach (PlayerControl p in Arsonist.dousedPlayers)
                    {
                        if (Arsonist.dousedIcons.ContainsKey(p.PlayerId))
                        {
                            Arsonist.dousedIcons[p.PlayerId].setSemiTransparent(false);
                        }
                    }
                }
            );

            // Set the default (or settings from the previous game) timers/durations when spawning the buttons
            setCustomButtonCooldowns();
        }
    }
}