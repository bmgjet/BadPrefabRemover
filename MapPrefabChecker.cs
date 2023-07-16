using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Windows.Forms;

namespace MapPrefabChecker
{
    public partial class Form1 : Form
    {

        WorldSerialization worldSerialization;
        Dictionary<uint,string> badprefabs = new Dictionary<uint, string>();

        public Form1(){InitializeComponent();}

        private void Form1_Load(object sender, EventArgs e)
        {
            //Download White List
            WebClient webClient = new WebClient();
            string prefablist = webClient.DownloadString("https://raw.githubusercontent.com/bmgjet/BadPrefabRemover/main/badprefablist");
            if (string.IsNullOrEmpty(prefablist))
            {
                MessageBox.Show("Failed to download bad prefab list");
                return;
            }
            //Split Data Out
            string[] lines = prefablist.Split(new string[] { "\n" }, StringSplitOptions.None);
            foreach (var l in lines)
            {
                string[] lines2 = l.Split(new string[] { " - " }, StringSplitOptions.None);
                if (lines2.Length == 2)
                {
                    uint id;
                    if (uint.TryParse(lines2[0], out id))
                    {
                        if (lines2[1].Contains("/"))
                        {
                            string[] lines3 = lines2[1].Split(new string[] { "/" }, StringSplitOptions.None);
                            badprefabs.Add(id, lines3[lines3.Count() - 1]);
                        }
                        else{badprefabs.Add(id, lines2[1]);}
                    }
                }
            }
            MessageBox.Show("Loaded Bad Prefab List (" + badprefabs.Count + " Known)","Map Prefab Checker",MessageBoxButtons.OK,MessageBoxIcon.Asterisk);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            //Save Button Disable
            button2.Enabled = false;
            //Load Map
            worldSerialization = new WorldSerialization();
            OpenFileDialog dlg = new OpenFileDialog();
            dlg.Filter = "Map files (*.map)|*.map";
            dlg.Title = "Open map file";
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                listBox1.Items.Clear();
                this.Text = "MapPrefabChecker - " +  Path.GetFileName(dlg.FileName);
                    worldSerialization.Load(dlg.FileName);
                    foreach (var p in worldSerialization.world.prefabs.ToArray())
                    {
                        //Check prefab is bad
                        if (badprefabs.ContainsKey(p.id))
                        {
                        p.category = badprefabs[p.id]; //Give map marker name
                        //Add to output
                        listBox1.Items.Add(p.id.ToString() + " - " + badprefabs[p.id] +"@ (X:" + p.position.x.ToString()+ " Y:" + p.position.y.ToString() + " Z:"+ p.position.z.ToString() + ")");
                        p.id = 1724395471; //Change prefab id
                    }
                        //Fix nan pos issues (Maps never be larger then vanilla network grid)
                        if (p.position.x > 4096){p.position.x = 4096;}
                        if (p.position.x < -4096){p.position.x = -4096;}
                        if (p.position.z > 4096){p.position.z = 4096;}
                        if (p.position.z < -4096){p.position.z = -4096;}
                        if (p.position.y >= 1000){p.position.y = 1000;}
                        if (p.position.y <= -500){p.position.y = -500;}
                    }
                    //Enable save button
                    button2.Enabled = true;
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            //Stip loot and vending profiles from map
            foreach (var b in worldSerialization.world.maps.ToArray())
            {
                string result = System.Text.Encoding.UTF8.GetString(b.data);
                if (result.Contains("SerializedLootableContainerData")){worldSerialization.world.maps.Remove(b);}
                if (result.Contains("SerializedVendingContainerData")){worldSerialization.world.maps.Remove(b);}
            }
            MessageBox.Show("Completed!", "Map Prefab Checker", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void button5_Click(object sender, EventArgs e)
        {
            //Remove all rustedit data and custom prefabs
            foreach (var p in worldSerialization.world.prefabs.ToArray())
            {
                //If not vanilla prefab remove it
                if(p.category.Contains("://")){worldSerialization.world.prefabs.Remove(p);}
                if (string.IsNullOrEmpty(p.category) || (p.category != "Decor" && p.category != "Dungeon" && p.category != "DungeonBase" && p.id != 1724395471)) //Monument marker
                {
                    p.category = "Decor";
                }
            }
            foreach (var b in worldSerialization.world.maps.ToArray())
            {
                //If not vanilla data remove it
                if (b.name != "height" && b.name != "terrain" && b.name != "splat" && b.name != "alpha" && b.name != "biome" && b.name != "topology" && b.name != "water")
                {
                    worldSerialization.world.maps.Remove(b);
                }
            }
            MessageBox.Show("Completed!", "Map Prefab Checker", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void button2_Click(object sender, EventArgs e)
        {
            //Save Map File
            SaveFileDialog oSaveFileDialog = new SaveFileDialog();
            oSaveFileDialog.Filter = "Map files (*.map)|*.map";
            oSaveFileDialog.Title = "Save map file";
            if (oSaveFileDialog.ShowDialog() == DialogResult.OK)
            {
                string fileName = oSaveFileDialog.FileName;
                string extesion = Path.GetExtension(fileName);
                switch (extesion)
                {
                    case ".map"://do something here 
                        worldSerialization.Save(fileName);
                        MessageBox.Show("File Saved!", "Map Prefab Checker", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                        break;
                    default://do something here
                        break;
                }
            }
        }
    }
}
