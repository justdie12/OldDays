using System;
using Content.Server.Administration;
using Content.Server.Administration.Managers;
using Content.Server.Atmos.Piping.Components;
using Content.Server.Atmos.Piping.EntitySystems;
using Content.Server.NodeContainer;
using Content.Server.NodeContainer.NodeGroups;
using Content.Shared.Administration;
using Robust.Server.Player;
using Robust.Shared.Console;
using Robust.Shared.GameObjects;
using Robust.Shared.IoC;
using Robust.Shared.Localization;
using Robust.Shared.Maths;

namespace Content.Server.Sandbox.Commands
{
    [AnyCommand]
    public sealed class ColorNetworkCommand : IConsoleCommand
    {
        public string Command => "colornetwork";
        public string Description => Loc.GetString("color-network-command-description");
        public string Help => Loc.GetString("color-network-command-help-text", ("command",Command));

        public void Execute(IConsoleShell shell, string argStr, string[] args)
        {
            var sandboxManager = IoCManager.Resolve<ISandboxManager>();
            var adminManager = IoCManager.Resolve<IAdminManager>();
            if (shell.IsClient && (!sandboxManager.IsSandboxEnabled && !adminManager.HasAdminFlag((IPlayerSession)shell.Player!, AdminFlags.Mapping)))
            {
                shell.WriteError("You are not currently able to use mapping commands.");
            }

            if (args.Length != 3)
            {
                shell.WriteLine(Loc.GetString("shell-wrong-arguments-number"));
                return;
            }



            var entityManager = IoCManager.Resolve<IEntityManager>();

            if (!int.TryParse(args[0], out var targetId))
            {
                shell.WriteLine(Loc.GetString("shell-argument-must-be-number"));
                return;
            }

            var eUid = new EntityUid(targetId);

            if (!eUid.IsValid() || !entityManager.EntityExists(eUid))
            {
                shell.WriteLine(Loc.GetString("shell-invalid-entity-id"));
                return;
            }

            if (!entityManager.TryGetComponent(eUid, out NodeContainerComponent? nodeContainerComponent))
            {
                shell.WriteLine(Loc.GetString("shell-entity-is-not-node-container"));
                return;
            }

            if (!Enum.TryParse(args[1], out NodeGroupID nodeGroupId))
            {
                shell.WriteLine(Loc.GetString("shell-node-group-is-invalid"));
                return;
            }

            var color = Color.TryFromHex(args[2]);
            if (!color.HasValue)
            {
                shell.WriteError(Loc.GetString("shell-invalid-color-hex"));
                return;
            }

            PaintNodes(nodeContainerComponent, nodeGroupId, color.Value);
        }

        private void PaintNodes(NodeContainerComponent nodeContainerComponent, NodeGroupID nodeGroupId, Color color)
        {
            var group = nodeContainerComponent.Nodes[nodeGroupId.ToString().ToLower()].NodeGroup;

            if (group == null) return;

            foreach (var x in group.Nodes)
            {
                if (!IoCManager.Resolve<IEntityManager>().TryGetComponent<AtmosPipeColorComponent?>(x.Owner, out var atmosPipeColorComponent)) continue;

                EntitySystem.Get<AtmosPipeColorSystem>().SetColor(x.Owner, atmosPipeColorComponent, color);
            }
        }
    }
}
