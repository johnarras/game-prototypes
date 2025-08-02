using Genrpg.Shared.Client.Core;
using Genrpg.Shared.Crawler.Info.Constants;
using Genrpg.Shared.Crawler.Info.EffectHelpers;
using Genrpg.Shared.Crawler.Info.InfoHelpers;
using Genrpg.Shared.Crawler.Spells.Settings;
using Genrpg.Shared.Entities.Interfaces;
using Genrpg.Shared.Entities.Services;
using Genrpg.Shared.HelperClasses;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.UI.Constants;
using Assets.Scripts.UI.Interfaces;
using Genrpg.Shared.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Assets.Scripts.UI.Constants;

namespace Genrpg.Shared.Crawler.Info.Services
{

    public class InfoOverviewPage
    {
        public string Header;
        public List<string> Lines;
    }

    public interface IInfoService : IInjectable
    {
        List<string> GetInfoLines(long entityTypeId, long entityId);
        string CreateInfoLink(IIdName idname, string nameShown = "");
        string CreateOverviewLink(string typeName);
        List<string> GetInfoLines(string entityLink); 
        string GetEffectText(CrawlerSpell spell, CrawlerSpellEffect effect);
        void SetupOverviewPages(string overviewText);
        List<InfoOverviewPage> GetOverviewPages();
        List<string> GetOverviewLines(string entityTypeName);
        string CreateHeaderLine(string headerText, bool makePlural = true);
           
    }
    
    public class InfoService : IInfoService
    {
        private ITextService _textService;
        private IEntityService _entityService;
        private IClientGameState _gs;       

        private SetupDictionaryContainer<long,IInfoHelper> _infoHelperDict = new SetupDictionaryContainer<long, IInfoHelper> ();
        private SetupDictionaryContainer<long,ISpellEffectHelper> _spellEffectDict = new SetupDictionaryContainer<long, ISpellEffectHelper> ();

        private Dictionary<string,List<string>> _overviewLines = new Dictionary<string, List<string>> ();

        private List<InfoOverviewPage> _overviewPages = new List<InfoOverviewPage>();

        private string pageBreak = "========";
        private string overviewEntityId = "overview";
        private string listAllText = "listall";
        public List<string> GetInfoLines(long entityTypeId, long entityId)
        {
            if (_infoHelperDict.TryGetValue (entityTypeId, out IInfoHelper info))
            {
                List<string> lines = info.GetInfoLines(entityId);

                IEntityHelper helper = _entityService.GetEntityHelper(entityTypeId);

                if (helper != null)
                {
                    lines.Insert(0, CreateOverviewLink(helper.GetChildType().Name));
                }

                for (int l = 0; l < lines.Count; l++)
                {
                    if (lines[l] == null)
                    {
                        Debug.Log("Null line?");
                        continue;
                    }
                    if (lines[l].IndexOf(InfoConstants.LinkPrefix) == 0)
                    {
                        lines[l] = " " + lines[l];
                    }
                }

                return lines;
            }

            return new List<string> ();
        }

        public string GetEffectText(CrawlerSpell spell, CrawlerSpellEffect effect)
        {
            if (_spellEffectDict.TryGetValue(effect.EntityTypeId, out ISpellEffectHelper helper))
            {
                return helper.ShowEffectInfo(spell, effect);
            }
            return "";
        }


        public string CreateInfoLink(IIdName idname, string nameShown = "")
        {
            if (idname == null)
            {
                return "";
            }

            if (string.IsNullOrEmpty(nameShown))
            {
                nameShown = idname.Name;
            }
            string linkId = idname.GetType().Name + " " + idname.IdKey;
            return CreateInfoLink(linkId, nameShown);
        }

        private string CreateInfoLink(string linkId, string nameShown)
        { 
            return InfoConstants.LinkPrefix + linkId + InfoConstants.LinkMiddle + _textService.HighlightText(StrUtils.SplitOnCapitalLetters(nameShown), TextColors.ColorYellow) + InfoConstants.LinkSuffix;
        }

        public List<string> GetInfoLines(string entityLink)
        {
            if (string.IsNullOrEmpty(entityLink))
            {
                return new List<string>();
            }

            string[] words = entityLink.Split (' ');

            if (words.Length < 1 || string.IsNullOrEmpty(words[0]) || string.IsNullOrEmpty(words[1]))
            {
                return new List<string>();
            }
          

            if (Int64.TryParse(words[1], out long entityId))
            {
                foreach (IInfoHelper helper in _infoHelperDict.GetDict().Values)
                {
                    if (helper.GetTypeName() == words[0])
                    {
                        return GetInfoLines(helper.Key, entityId);
                    }
                }
            }
            else if (words[1].ToLower() == overviewEntityId)
            { 
                if (_overviewLines.TryGetValue(words[0].ToLower(), out List<string> lines))
                {
                    return lines;
                }
            }

            return new List<string>();
        }

        public List<string> GetOverviewLines(string entityTypeName)
        {
            if (_overviewLines.ContainsKey(entityTypeName.ToLower()))
            {
                return _overviewLines[entityTypeName.ToLower()];
            }
            return new List<string>();
        }

        public void SetupOverviewPages(string overviewText)
        {

            if (_overviewPages.Count > 0)
            {
                return;
            }

            string[] lines = overviewText.Split('\n');

            List<string> currPageLines = new List<string>();

            List<string> overviewKeys = new List<string>();
            List<string> overviewChildText = new List<string>();
            string overviewHeader = "";

            for (int i = 0; i < lines.Length; i++)
            {
                if (lines[i].IndexOf(pageBreak) == -1 && i != lines.Length-1)
                {
                    currPageLines.Add(lines[i]);
                }
                else
                {
                    if (currPageLines.Count < 1)
                    {
                        continue;
                    }

                    if (overviewChildText.Count > 0)
                    {
                        currPageLines.AddRange(overviewChildText);
                    }
                    
                    foreach (string key in overviewKeys)
                    {
                        List<string> newPageLines = new List<string>();
                        overviewHeader = SanitizeName(key, true);
                        newPageLines.Add(overviewHeader);
                        newPageLines.AddRange(currPageLines);
                        _overviewLines[StrUtils.NormalizeWord(key)] = newPageLines;
                    }

                    if (currPageLines.Count > 0)
                    {

                        _overviewPages.Add(new InfoOverviewPage()
                        {
                            Header = overviewHeader,
                            Lines = currPageLines,
                        });
                    }

                    overviewChildText.Clear();
                    overviewKeys.Clear();
                    overviewHeader = "";

                    currPageLines = new List<string>();

                    string[] words = lines[i].Split(' ');

                    List<string> origWords = new List<string>(words);

                    for (int w = 0; w < words.Length; w++)
                    {
                        words[w] = StrUtils.NormalizeWord(words[w]);
                    }

                    bool shouldListAll = words.Any(x => x == listAllText);

                    overviewKeys = origWords.Where(x=>x != pageBreak && !StrUtils.NormalizeWord(x).Contains(listAllText)).ToList();    

                    // Set up overview + children link.
                    if (words.Length >= 3 && words.Any(x => x == listAllText))
                    {
                        for (int w = 1; w < words.Length; w++)
                        {

                            if (overviewChildText.Count > 0)
                            {
                                break;
                            }
                            IEntityHelper helper = _entityService.GetEntityHelper(words[w]);

                            if (helper == null)
                            {
                                continue;
                            }

                            List<IIdName> children = helper.GetChildList(_gs.ch);

                            if (_infoHelperDict.TryGetValue(helper.Key, out IInfoHelper infoHelper))
                            {
                                children = infoHelper.GetInfoChildren();
                            }

                            children = children.OrderBy(x=>x.Name).ToList();

                            if (children.Count > 0)
                            {
                                StringBuilder sb = new StringBuilder();
                                for (int c = 0; c < children.Count; c++)
                                {
                                    overviewChildText.Add(" " + CreateInfoLink(children[c]));
                                }
                            }
                        }
                    }
                }
            }

            if (currPageLines.Count > 0)
            {

                _overviewPages.Add(new InfoOverviewPage()
                {
                    Header = overviewHeader,
                    Lines = currPageLines,
                });
            }
        }

        public List<InfoOverviewPage> GetOverviewPages()
        {
            return _overviewPages;
        }

        public string CreateOverviewLink(string typeName)
        {
            if (!_overviewLines.ContainsKey(typeName.ToLower()))
            {
                return typeName;
            }

            return "<align=\"center\">" 
                + "<size=+10px>" 
                + CreateInfoLink(typeName + " " + overviewEntityId, SanitizeName(typeName, true))
                + "</size>"
                + "</align>";
        }

        private string SanitizeName(string name, bool makePlural)
        {
            if (makePlural)
            {
                return StrUtils.SplitOnCapitalLetters(StrUtils.MakePlural(name.Replace("Type", "").Replace("type", "")));
            }
            else
            {
                return StrUtils.SplitOnCapitalLetters(name.Replace("Type", "").Replace("type", ""));
            }
        }

        public string CreateHeaderLine(string headerText, bool makePlural = true)
        {
            return "<align=\"center\">"
                + "<size=+10px>"
                + SanitizeName(headerText, makePlural)
                + "</size>"
                + "</align>";
        }
    }
}
