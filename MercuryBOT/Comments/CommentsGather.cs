﻿/*  
 ▄▀▀▄ ▄▀▄  ▄▀▀█▄▄▄▄  ▄▀▀▄▀▀▀▄  ▄▀▄▄▄▄   ▄▀▀▄ ▄▀▀▄  ▄▀▀▄▀▀▀▄  ▄▀▀▄ ▀▀▄ 
█  █ ▀  █ ▐  ▄▀   ▐ █   █   █ █ █    ▌ █   █    █ █   █   █ █   ▀▄ ▄▀ 
▐  █    █   █▄▄▄▄▄  ▐  █▀▀█▀  ▐ █      ▐  █    █  ▐  █▀▀█▀  ▐     █   
  █    █    █    ▌   ▄▀    █    █        █    █    ▄▀    █        █   
▄▀   ▄▀    ▄▀▄▄▄▄   █     █    ▄▀▄▄▄▄▀    ▀▄▄▄▄▀  █     █       ▄▀    
█    █     █    ▐   ▐     ▐   █     ▐             ▐     ▐       █     
▐    ▐     ▐                  ▐                                 ▐   
*/

using System;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using System.Collections.Generic;

using AngleSharp.Html.Parser;
using SteamComments;
using System.Threading;
using AngleSharp.Text;
using MercuryBOT.Helpers;


namespace MercuryBOT
{
    public partial class CommentsGather : MetroFramework.Forms.MetroForm
    {

        private readonly WebClient Web = new WebClient();
        private string ProfileOrClan;

        public CommentsGather()
        {
            InitializeComponent();
            this.FormBorderStyle = FormBorderStyle.None;
            this.components.SetStyle(this);
            Region = System.Drawing.Region.FromHrgn(Helpers.Extensions.CreateRoundRectRgn(0, 0, Width, Height, 5, 5));
        }
        private void CommentsGather_Load(object sender, EventArgs e)
        {
            // GatherComments();
            ProgressSpinner.Visible = false;

            lbl_totalCommentsInGrid.Visible = false;
            lbl_totalComments.Visible = false;
        }

        private void Combox_profileOrClan_SelectedIndexChanged(object sender, EventArgs e)
        {
            switch (combox_profileOrClan.SelectedIndex)
            {
                case 0:
                    ProfileOrClan = "Profile";
                    break;
                case 1:
                    ProfileOrClan = "Clan";
                    break;
            }
        }
        public static string RemoveSpecialCharacters(string str)
        {
            return Regex.Replace(str, "[^a-zA-Z0-9_.]+", " ", RegexOptions.Compiled);
        }

        public void GatherComments()
        {
            //  if (AccountLogin.IsLoggedIn == true)
            //  {

            lbl_totalCommentsInGrid.Invoke((MethodInvoker)delegate
            {
                lbl_totalCommentsInGrid.Text = "Total Row Count:0";
            });
            lbl_totalComments.Invoke((MethodInvoker)delegate
            {
                lbl_totalComments.Text = "Total Count: 0";
            });


            ProgressSpinner.Invoke((MethodInvoker)delegate
            {
                ProgressSpinner.Visible = true;
            });
            if (string.IsNullOrEmpty(ProfileOrClan) || string.IsNullOrEmpty(txtBox_profileGroupID.Text) || string.IsNullOrEmpty(txtBox_Comments2GetCount.Text))
            {
                InfoForm.InfoHelper.CustomMessageBox.Show("Info", "Please select or write the profile/group url.");
                return;
            }
            GridCommentsData.Invoke((MethodInvoker)delegate
            {
                GridCommentsData.Rows.Clear();
            });


            btn_doTask.Invoke((MethodInvoker)delegate
            {
                btn_doTask.Enabled = false;
            });

            try
            {
                string ProfileORGroupComments = "https://steamcommunity.com/comment/" + ProfileOrClan + "/render/" + txtBox_profileGroupID.Text + "/-1/?count=" + txtBox_Comments2GetCount.Text;

                // string GroupComments = "https://steamcommunity.com/comment/Clan/render/103582791434391876/-1/?count=10";
                // string pasha = "https://steamcommunity.com/comment/Profile/render/76561197973845818/-1/?count=500";
                // string ProfileComments = "https://steamcommunity.com/comment/Profile/render/76561198041931474/-1/?count=10";

                var parser = new HtmlParser();

                var json = Web.DownloadString(ProfileORGroupComments);
                var renderComments = RenderComments.FromJson(json);

                var document = parser.ParseDocument(renderComments.CommentsHtml);
                var eCommentList = document.QuerySelectorAll("div.commentthread_comment");


                lbl_totalComments.Invoke((MethodInvoker)delegate
                {
                    lbl_totalComments.Text = "Total Count: " + renderComments.TotalCount.ToString();
                });


                foreach (var eComment in eCommentList)
                {
                    var CommentID = eComment.QuerySelector("div.commentthread_comment_text").GetAttribute("id").Replace("comment_content_", "");
                    var CommentContent = RemoveSpecialCharacters(eComment.QuerySelector("div.commentthread_comment_text").TextContent.Trim());
                    var Author = eComment.QuerySelector("a[class='hoverunderline commentthread_author_link']").GetAttribute("href").Replace("https://steamcommunity.com/profiles/", "").Replace("https://steamcommunity.com/id/", "");

                    var Time = eComment.QuerySelector("span.commentthread_comment_timestamp").GetAttribute("title").Replace("https://steamcommunity.com/profiles/", ""); // title=convertido
                    int index = Time.IndexOf("@"); if (index > 0) { Time = Time.Substring(0, index); } // remove hours, only stay date

                    string[] row = { CommentID, CommentContent, Author, Time };

                    GridCommentsData.Invoke((MethodInvoker)delegate
                    {
                        GridCommentsData.Rows.Add(row.Distinct().ToArray());
                    });


                    string[] arrayComments = CommentContent.Split(' ');

                    //bool result = Author.Any(x => !char.IsLetter(x));


                    if (chck_containsWords.Checked && txtBox_filterWords.Text.Length != 0)
                    {
                        foreach (string item in arrayComments)
                        {
                            string[] filterSelectedWords = txtBox_filterWords.Text.Split(',');
                            
                            if (chck_ignoreCase.Checked && filterSelectedWords.Contains(item, StringComparison.OrdinalIgnoreCase))
                            {
                                Console.WriteLine("AnyCase - DELETED: " + CommentID + " ||  " + CommentContent + "\n");
                                // AccountLogin.DeleteSelectedComment(CommentID, ProfileOrClan);


                                Thread.Sleep(5);
                            }
                            else if (filterSelectedWords.Contains(item))
                            {
                                Console.WriteLine("DELETED: " + CommentID + "\n");

                                // AccountLogin.DeleteSelectedComment(CommentID, ProfileOrClan);
                                Thread.Sleep(5);
                            }
                        }
                    }
                }
                
                lbl_totalCommentsInGrid.Invoke((MethodInvoker)delegate
                {
                    lbl_totalCommentsInGrid.Text = "Total Row Count:" + GridCommentsData.Rows.Count.ToString();
                });


                CommentsList_ScrollBar.Invoke((MethodInvoker)delegate
                {
                    CommentsList_ScrollBar.Maximum = GridCommentsData.Rows.Count;
                });

                ProgressSpinner.Invoke((MethodInvoker)delegate
                {
                    ProgressSpinner.Visible = false;
                });
                lbl_totalCommentsInGrid.Invoke((MethodInvoker)delegate
                {
                    lbl_totalCommentsInGrid.Visible = true;
                });
                lbl_totalComments.Invoke((MethodInvoker)delegate
                {
                    lbl_totalComments.Visible = true;
                });
                btn_doTask.Invoke((MethodInvoker)delegate
                {
                    btn_doTask.Enabled = true;
                });

            }
            catch (Exception e)
            {
                btn_doTask.Invoke((MethodInvoker)delegate
                {
                    btn_doTask.Enabled = true;
                });
                ProgressSpinner.Invoke((MethodInvoker)delegate
                {
                    ProgressSpinner.Visible = false;
                });


                Console.WriteLine(e);
            }
            //  }
            //  else
            //  {
            //  InfoForm.InfoHelper.CustomMessageBox.Show("Not logged!");
            // }
        }


        //if key down DELETE  -  AccountLogin.DeleteSelectedComment(CommentID, ProfileOrClan);


        public string Url2ID(string profileURL)
        {

            var parser = new HtmlParser();

            var html = Web.DownloadString("https://steamcommunity.com/id/sp0okER/");

            var document = parser.ParseDocument(html);
            //    var steamID64Clean = document.DocumentElement.QuerySelector("div[class='commentthread_paging has_view_all_link']").GetAttribute("id").Replace("commentthread_Profile_", "").Replace("_pagecontrols", "");
            var steamID64Clean = document.DocumentElement.QuerySelector("div[class='commentthread_paging has_view_all_link']").GetAttribute("id").Replace("commentthread_Profile_", "").Replace("_pagecontrols", "");


            return steamID64Clean;

            // }
            // catch (Exception)
            // {
            //     return "0";
            // }


        }

        public string IF_PROFILE_PRIVATE(string ProfileURL)// meter api key da config
        {
            var html = Web.DownloadString("http://api.steampowered.com/ISteamUser/ResolveVanityURL/v0001/?key=&vanityurl=" + ProfileURL);
            var steamID64Clean = html.Replace('"', ' ').Replace("{ response :{ steamid :", "").Replace(" , success :1}}", "").Trim();

            return steamID64Clean;
            //add try maybe

        }

        private void Btn_doTask_Click(object sender, EventArgs e)
        {
            //aa();
            //GatherComments();
            CollectComments.RunWorkerAsync();
        }

        private void CommentsGather_Shown(object sender, EventArgs e)
        {

        }

        private void CollectComments_DoWork(object sender, DoWorkEventArgs e)
        {
            GatherComments();
        }

        private void CommentsGather_FormClosed(object sender, FormClosedEventArgs e)
        {
            GC.Collect();
            GC.WaitForPendingFinalizers();
        }

        private void CommentsList_ScrollBar_Scroll(object sender, ScrollEventArgs e)
        {
            if (e.NewValue >= GridCommentsData.Rows.Count)
            {
                return;
            }
            GridCommentsData.FirstDisplayedScrollingRowIndex = e.NewValue;
        }
    }
}