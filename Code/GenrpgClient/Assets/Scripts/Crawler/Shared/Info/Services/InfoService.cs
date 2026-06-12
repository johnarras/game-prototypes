using Assets.Scripts.ClientEvents;
using Assets.Scripts.UI.Constants;
using Assets.Scripts.UI.Interfaces;
using OxDb.SharedCore.Entities.Interfaces;
using OxDb.SharedCore.Entities.Services;
using OxDb.SharedCore.HelperClasses;
using OxDb.SharedCore.Interfaces;
using OxDb.SharedCore.Utils;
using OxDb.SharedGame.Crawler.Info.Constants;
using OxDb.SharedGame.Crawler.Info.EffectHelpers;
using OxDb.SharedGame.Crawler.Info.InfoHelpers;
using OxDb.SharedGame.Crawler.Spells.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OxDb.SharedGame.Crawler.Info.Services
{

    public interface IInfoService : IInjectable
    {
        ShowInfoPanelArgs GetInfoPanelArgs(long entityTypeId, long entityId);
        string CreateInfoLink(IIdName idname, string nameShown = "");
        string CreateOverviewLink(string typeName, bool makePlural);
        ShowInfoPanelArgs GetInfoPanelArgs(string entityLink);
        string GetEffectText(CrawlerSpell spell, CrawlerSpellEffect effect);
        void SetupOverviewPages(string overviewText);
        List<ShowInfoPanelArgs> GetOverviewPages();
        string CreateHeaderLine(string headerText, bool makePlural = true);
        IInfoHelper GetInfoHelper(long entityTypeId);
    }

    public class InfoService : IInfoService
    {
        private ITextService _textService = null;
        private IEntityService _entityService = null;
        private IClientGameState _gs = null;

        private SetupDictionaryContainer<long, IInfoHelper> _infoHelperDict = new SetupDictionaryContainer<long, IInfoHelper>();
        private SetupDictionaryContainer<long, ISpellEffectHelper> _spellEffectDict = new SetupDictionaryContainer<long, ISpellEffectHelper>();

        private Dictionary<string, List<string>> _overviewLines = new Dictionary<string, List<string>>();

        private List<ShowInfoPanelArgs> _overviewPages = new List<ShowInfoPanelArgs>();

        private string pageBreak = "========";
        private string overviewEntityId = "overview";
        private string listAllText = "listall";
        public ShowInfoPanelArgs GetInfoPanelArgs(long entityTypeId, long entityId)
        {
            ShowInfoPanelArgs args = new ShowInfoPanelArgs()
            {
                EntityTypeId = entityTypeId,
                EntityId = entityId
            };

            if (_infoHelperDict.TryGetValue(entityTypeId, out IInfoHelper info))
            {
                List<string> lines = info.GetInfoLines(entityId);

                IEntityHelper helper = _entityService.GetEntityHelper(entityTypeId);

                if (helper != null)
                {
                    lines.Insert(0, CreateOverviewLink(helper.GetChildType().Name, info.OverviewTypeNameIsPlural()));
                }

                for (int l = 0; l < lines.Count; l++)
                {
                    if (lines[l] == null)
                    {
                        continue;
                    }
                    if (lines[l].IndexOf(InfoConstants.LinkPrefix) == 0)
                    {
                        lines[l] = " " + lines[l];
                    }
                }

                args.Lines = lines;
                return args;
            }

            return args;
        }


        public IInfoHelper GetInfoHelper(long entityTypeId)
        {
            if (_infoHelperDict.TryGetValue(entityTypeId, out IInfoHelper helper))
            {
                return helper;
            }
            return null;
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

        public ShowInfoPanelArgs GetInfoPanelArgs(string entityLink)
        {
            ShowInfoPanelArgs args = new ShowInfoPanelArgs();

            if (string.IsNullOrEmpty(entityLink))
            {
                return args;
            }

            string[] words = entityLink.Split(' ');

            if (words.Length < 1 || string.IsNullOrEmpty(words[0]) || string.IsNullOrEmpty(words[1]))
            {
                return args;
            }

            if (Int64.TryParse(words[1], out long entityId))
            {
                foreach (IInfoHelper helper in _infoHelperDict.GetDict().Values)
                {
                    if (helper.GetTypeName() == words[0])
                    {
                        return GetInfoPanelArgs(helper.HelperKey, entityId);
                    }
                }
            }
            else if (words[1].ToLower() == overviewEntityId)
            {
                if (_overviewLines.TryGetValue(words[0].ToLower(), out List<string> lines))
                {
                    args.Lines = lines;
                    return args;
                }
            }

            return args;
        }

        public void SetupOverviewPages(string overviewText)
        {

            if (_overviewPages.Count > 0)
            {
                return;
            }

            List<string> lines = StrUtils.SplitIntoLines(overviewText);

            List<string> currPageLines = new List<string>();

            List<string> overviewKeys = new List<string>();
            List<string> overviewChildText = new List<string>();
            string overviewHeader = "";

            for (int i = 0; i < lines.Count; i++)
            {
                if (lines[i].IndexOf(pageBreak) == -1 && i != lines.Count - 1)
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

                        _overviewPages.Add(new ShowInfoPanelArgs()
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

                    bool shouldListAll = words.FastAny(x => x == listAllText);

                    overviewKeys = origWords.Where(x => x != pageBreak && !StrUtils.NormalizeWord(x).Contains(listAllText)).ToList();

                    // Set up overview + children link.
                    if (words.Length >= 3 && words.FastAny(x => x == listAllText))
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

                            if (_infoHelperDict.TryGetValue(helper.HelperKey, out IInfoHelper infoHelper))
                            {
                                children = infoHelper.GetInfoChildren();
                            }


                            children = children.OrderBy(x => x.Name).ToList();

                            if (infoHelper != null && !infoHelper.OrderByName)
                            {
                                children = children.OrderBy(x => x.IdKey).ToList();
                            }

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

                _overviewPages.Add(new ShowInfoPanelArgs()
                {
                    Header = overviewHeader,
                    Lines = currPageLines,
                });
            }
        }

        public List<ShowInfoPanelArgs> GetOverviewPages()
        {
            return _overviewPages;
        }

        public string CreateOverviewLink(string typeName, bool makePlural)
        {
            if (!_overviewLines.ContainsKey(typeName.ToLower()))
            {

                return "<align=\"center\">"
                    + "<size=+10px>"
                    + StrUtils.SplitOnCapitalLetters(typeName.Replace("Type", ""))
                    + "</size>"
                    + "</align>";
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
            return "\n<align=\"center\">"
                + "<size=+10px>"
                + SanitizeName(headerText, makePlural)
                + "</size>"
                + "</align>";
        }
    }
}


