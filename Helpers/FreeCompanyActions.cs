using System.Linq;
using System.Threading.Tasks;
using System.Windows.Media;
using Buddy.Coroutines;
using ff14bot.Enums;
using LlamaLibrary.Logging;
using LlamaLibrary.RemoteAgents;
using LlamaLibrary.RemoteWindows;

namespace LlamaLibrary.Helpers
{
    /// <summary>
    /// Provides a helper for activating Free Company buffs (actions).
    /// Opens the Free Company panel, purchases any missing buff actions from the GC shop if needed,
    /// then activates the requested buffs via the <see cref="FreeCompanyAction"/> window.
    /// </summary>
    public static class FreeCompanyActions
    {
        private static readonly LLogger Log = new(nameof(FreeCompanyActions), Colors.DarkTurquoise);

        /// <summary>
        /// Ensures that Free Company buffs <paramref name="buff1"/> and <paramref name="buff2"/> are active.
        /// If either buff has not been purchased it will be bought from the Grand Company shop using
        /// <see cref="GrandCompanyHelper.BuyFCAction"/> before activation.
        /// Does nothing if both buffs are already running.
        /// </summary>
        /// <param name="buff1">Action ID of the first FC buff to activate.</param>
        /// <param name="buff2">Action ID of the second FC buff to activate.</param>
        /// <param name="grandCompany">The player's Grand Company, used when purchasing missing actions.</param>
        public static async Task ActivateBuffs(int buff1, int buff2, GrandCompany grandCompany)
        {
            if (!FreeCompany.Instance.IsOpen)
            {
                AgentFreeCompany.Instance.Toggle();
                await Coroutine.Wait(5000, () => FreeCompany.Instance.IsOpen);
            }

            var curActions = await AgentFreeCompany.Instance.GetCurrentActions();
            var fcActions = await AgentFreeCompany.Instance.GetAvailableActions();

            if (curActions.Length == 2)
            {
                if (FreeCompany.Instance.IsOpen)
                {
                    FreeCompany.Instance.Close();
                }

                return;
            }

            await GeneralFunctions.StopBusy(dismount: false);
            var buffs1 = fcActions.Select((n, index) => new { Action = n, Index = index }).FirstOrDefault(n => n.Action.id == buff1);
            var buffs2 = fcActions.Select((n, index) => new { Action = n, Index = index }).FirstOrDefault(n => n.Action.id == buff2);

            if (buffs1 == null && !curActions.Any(i => i.id == buff1))
            {
                if (FreeCompany.Instance.IsOpen)
                {
                    FreeCompany.Instance.Close();
                }

                await GrandCompanyHelper.BuyFCAction(grandCompany, buff1);
                await Coroutine.Sleep(1000);

                Log.Verbose("Bought buff1");
                if (!FreeCompany.Instance.IsOpen)
                {
                    Log.Verbose("Opening window after buy");
                    AgentFreeCompany.Instance.Toggle();
                    await Coroutine.Wait(5000, () => FreeCompany.Instance.IsOpen);
                    if (FreeCompany.Instance.IsOpen)
                    {
                        Log.Verbose("Buff 1 bought checking again");
                        FreeCompany.Instance.SelectActions();
                        await Coroutine.Wait(5000, () => FreeCompanyAction.Instance.IsOpen);
                        fcActions = await AgentFreeCompany.Instance.GetAvailableActions();
                        buffs1 = fcActions.Select((n, index) => new { Action = n, Index = index }).FirstOrDefault(n => n.Action.id == buff1);
                    }
                }
            }

            await Coroutine.Sleep(500);
            fcActions = await AgentFreeCompany.Instance.GetAvailableActions();
            buffs2 = fcActions.Select((n, index) => new { Action = n, Index = index }).FirstOrDefault(n => n.Action.id == buff2);
            if (buffs2 == null && !curActions.Any(i => i.id == buff2))
            {
                if (FreeCompany.Instance.IsOpen)
                {
                    FreeCompany.Instance.Close();
                }

                await GrandCompanyHelper.BuyFCAction(grandCompany, buff2);
                await Coroutine.Sleep(1000);

                Log.Verbose("Bought buff2");
                if (!FreeCompany.Instance.IsOpen)
                {
                    Log.Verbose("Opening window after buy");
                    AgentFreeCompany.Instance.Toggle();
                    await Coroutine.Wait(5000, () => FreeCompany.Instance.IsOpen);
                    if (FreeCompany.Instance.IsOpen)
                    {
                        Log.Verbose("Buff 2 bought checking again");
                        FreeCompany.Instance.SelectActions();
                        await Coroutine.Wait(5000, () => FreeCompanyAction.Instance.IsOpen);
                        fcActions = await AgentFreeCompany.Instance.GetAvailableActions();
                        buffs2 = fcActions.Select((n, index) => new { Action = n, Index = index }).FirstOrDefault(n => n.Action.id == buff2);
                    }
                }
            }

            if (curActions.Length == 0)
            {
                Log.Verbose("No Buffs: Activating");
                if (!FreeCompanyAction.Instance.IsOpen)
                {
                    FreeCompany.Instance.SelectActions();
                    await Coroutine.Wait(5000, () => FreeCompanyAction.Instance.IsOpen);
                }

                if (FreeCompanyAction.Instance.IsOpen)
                {
                    if (buffs1 != null)
                    {
                        await FreeCompanyAction.Instance.EnableAction(buffs1.Index);
                    }

                    await Coroutine.Sleep(500);
                    fcActions = await AgentFreeCompany.Instance.GetAvailableActions();
                    buffs2 = fcActions.Select((n, index) => new { Action = n, Index = index }).FirstOrDefault(n => n.Action.id == buff2);
                    if (buffs2 != null)
                    {
                        await FreeCompanyAction.Instance.EnableAction(buffs2.Index);
                    }
                }
            }
            else
            {
                if (!curActions.Any(i => i.id == buff1))
                {
                    Log.Information("Buff 1 not active");
                    if (!FreeCompanyAction.Instance.IsOpen)
                    {
                        FreeCompany.Instance.SelectActions();
                        await Coroutine.Wait(5000, () => FreeCompanyAction.Instance.IsOpen);
                    }

                    if (FreeCompanyAction.Instance.IsOpen)
                    {
                        if (buffs1 != null)
                        {
                            await FreeCompanyAction.Instance.EnableAction(buffs1.Index);
                        }
                    }
                }
                else
                {
                    Log.Information("Buff 2 not active");
                    if (!FreeCompanyAction.Instance.IsOpen)
                    {
                        FreeCompany.Instance.SelectActions();
                        await Coroutine.Wait(5000, () => FreeCompanyAction.Instance.IsOpen);
                    }

                    if (FreeCompanyAction.Instance.IsOpen)
                    {
                        if (buffs2 != null)
                        {
                            await FreeCompanyAction.Instance.EnableAction(buffs2.Index);
                        }
                    }
                }
            }

            if (FreeCompany.Instance.IsOpen)
            {
                FreeCompany.Instance.Close();
            }
        }
    }
}