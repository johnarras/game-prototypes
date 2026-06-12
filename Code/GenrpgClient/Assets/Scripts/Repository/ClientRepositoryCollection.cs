using Assets.Scripts.Repository;
using OxDb.SharedCore.DataStores.Entities;
using OxDb.SharedCore.Interfaces;
using OxDb.SharedCore.Logalytics.Interfaces;
using OxDb.SharedCore.Serialization.Interfaces;
using OxDb.SharedCore.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq.Expressions;
using System.Threading.Tasks;

public interface IClientRepositoryCollection
{
    Task<bool> Save(object t, RepoSaveArgs args = null);
    Task<object> LoadWithType(Type t, string id);
}

public class ClientRepositoryCollection<T> : IClientRepositoryCollection where T : class, IStringId
{

    private ILogService _logService = null;
    private ITextSerializer _serializer = null;
    private IClientRepositoryService _repoService = null;
    private IClientCryptoService _clientCryptoService = null;

    /// <summary>
    /// Construct a ClientRepositoryCollection. This happens in one spot in ClientRepositoryService
    /// creating a generic type using Activator.CreateInstance, the args here must match the ones in that code.
    /// Look for REPOCREATEARGSYNC to see where this constructor is called.
    /// </summary>
    /// <param name="repoService"></param>
    /// <param name="logService"></param>
    /// <param name="serializer"></param>
    /// <param name="clientCryptoService"></param>
    public ClientRepositoryCollection(IClientRepositoryService repoService, ILogService logService,
        ITextSerializer serializer, IClientCryptoService clientCryptoService)
    {
        _repoService = repoService;
        _logService = logService;
        _serializer = serializer;
        _clientCryptoService = clientCryptoService;
    }

    public virtual async Task<bool> SaveAll(List<T> list, RepoSaveArgs args = null)
    {
        if (list == null)
        {
            return false;
        }

        for (int i = 0; i < list.Count; i++)
        {
            await Save(list[i], args);
        }
        return true;
    }

    private string GetKeyFromId(string id)
    {
        return typeof(T).Name + id;
    }


    public async Task<T> Load(String id)
    {
        try
        {
            await Task.CompletedTask;
            if (string.IsNullOrEmpty(id))
            {
                return default(T);
            }
            string key = GetKeyFromId(id);
            string val = LoadString(key);
            if (string.IsNullOrEmpty(val))
            {
                return default(T);
            }
            return _serializer.Deserialize<T>(val);
        }
        catch (Exception e)
        {
            _logService.Exception(e, "Local Load Error");
            return default(T);
        }
    }

    public async Task<bool> Save(object t, RepoSaveArgs args = null)
    {
        return await SaveInternal(t, args);
    }

    private async Task<bool> SaveInternal(object t, RepoSaveArgs args = null)
    {
        if (t == null)
        {
            return false;
        }
        try
        {
            string id = "";
            if (t is IStringId tid)
            {
                id = tid.Id;
            }
            string key = GetKeyFromId(id);

            if (args != null && !string.IsNullOrEmpty(args.OverrideId))
            {
                key = args.OverrideId;
            }

            bool shouldPrettyPrint = args != null && args.Verbose && !args.Encrypt;

            string val = shouldPrettyPrint ? _serializer.PrettyPrint(t) : _serializer.SerializeToString(t);

            SaveString(key, val, args);
        }
        catch (Exception e)
        {
            _logService.Exception(e, "Local Save Error");
            return false;
        }

        await Task.CompletedTask;
        return true;
    }
    public async Task<bool> Delete(T t)
    {
        if (t == null)
        {
            return false;
        }

        if (!(t is IStringId sid))
        {
            return false;
        }

        string id = sid.Id;

        if (string.IsNullOrEmpty(id))
        {
            return false;
        }
        string key = GetKeyFromId(id);
        try
        {
            DeleteString(key);
        }
        catch (Exception e)
        {
            _logService.Exception(e, "LocalRepository.Delete");
            return false;
        }
        await Task.CompletedTask;
        return true;
    }

    protected string GetPathPrefix()
    {
        return _repoService.GetPathPrefix();
    }


    private string GetPath(string id)
    {
        if (string.IsNullOrEmpty(id))
        {
            return "";
        }

        string basePath = GetPathPrefix();


        if (!Directory.Exists(basePath))
        {
            Directory.CreateDirectory(basePath);
        }


        if (id.LastIndexOf("/") >= 0)
        {
            string beforeSlash = id.Substring(0, id.LastIndexOf("/"));
            if (!string.IsNullOrEmpty(beforeSlash))
            {
                string fullDir = basePath + "/" + beforeSlash;
                if (!Directory.Exists(fullDir))
                {
                    Directory.CreateDirectory(fullDir);
                }
            }
        }
        return basePath + "/" + id;
    }


    public void DeleteAllData()
    {
        string pathPrefix = GetPathPrefix();
        if (Directory.Exists(pathPrefix))
        {
            Directory.Delete(pathPrefix, true);
        }
    }

    public async Task<T> LoadObjectFromString(string id)
    {
        string txt = LoadString(id);
        if (string.IsNullOrEmpty(txt))
        {
            return default(T);
        }
        await Task.CompletedTask;
        return _serializer.Deserialize<T>(txt);

    }

    public string LoadString(string id)
    {
        string path = GetPath(id);
        if (!File.Exists(path))
        {
            return "";
        }

        string finalText = null;

        try
        {

            string startText = File.ReadAllText(path, System.Text.Encoding.UTF8);

            try
            {
                finalText = _clientCryptoService.DecryptString(StrUtils.ConvertFromBase64(startText));
            }
            catch
            {
                finalText = startText;

            }
        }
        catch (Exception e)
        {
            _logService.Info("Failed to read file: " + path + " " + e.Message);
        }
        return finalText;
    }

    protected void SaveString(string id, string val, RepoSaveArgs args)
    {
        string path = GetPath(id);
        try
        {
            if (args != null && args.Encrypt)
            {
                val = StrUtils.ConvertToBase64(_clientCryptoService.EncryptString(val));
            }
            File.WriteAllText(path, val, System.Text.Encoding.UTF8);
        }
        catch (Exception e)
        {
            _logService.Info("Failed to save text file: " + path + " " + e.Message);
        }
    }

    public void DeleteString(string id)
    {
        string path = GetPath(id);
        try
        {
            File.Delete(path);
        }
        catch (Exception e)
        {
            _logService.Info("Failed to delete file: " + path + " " + e.Message);
        }
    }

    public async Task<List<T>> LoadAll(List<string> ids)
    {

        await Task.CompletedTask;
        throw new NotImplementedException();
    }

    public async Task<List<T>> Search(Expression<Func<T, bool>> func, int quantity = 100, int skip = 0)
    {

        await Task.CompletedTask;
        throw new NotImplementedException();
    }

    public async Task<object> LoadWithType(Type t, string id)
    {

        await Task.CompletedTask;
        try
        {
            if (string.IsNullOrEmpty(id))
            {
                return null;
            }
            string key = GetKeyFromId(id);
            string val = LoadString(key);
            if (string.IsNullOrEmpty(val))
            {
                return null;
            }
            return _serializer.DeserializeWithType(val, t);
        }
        catch (Exception e)
        {
            _logService.Exception(e, "Local LoadWithType Error");
            return default(T);
        }

    }
}


