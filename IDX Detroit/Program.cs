using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;

namespace IDX_Detroit
{
    class Program
    {
        static void Main(string[] args)
        {
            if(args.Length == 0 || args[0] != "-p" && args[0] != "-e")
            {
                Console.WriteLine("Usage extract: IDX_Detroit.exe -e idxfile id num_obj(0 is extract all)");
                Console.WriteLine("Exemple: IDX_Detroit.exe -e BigFile_PC.idx 19003 125");
                Console.WriteLine("Usage repack: IDX_Detroit.exe -p idxfile *.FileSizeTable");
                Console.WriteLine("Exemple: IDX_Detroit.exe -p BigFile_PC.idx e9c8981dc9a3096d504a063ed3fa3710.FileSizeTable");
                Console.ReadKey();
            }
            else
            {
                /*Console.WriteLine(Environment.CurrentDirectory);
                Console.ReadKey();*/

                if (!File.Exists(Environment.CurrentDirectory + "\\" + args[1]))
                {
                    Console.WriteLine("File {0} not exist!", args[1]);
                    Console.ReadKey();
                }
                else
                {
                    try
                    {
                        if (!string.IsNullOrEmpty(args[2]) && args[0] == "-e")
                        {
                            Console.WriteLine("File {0} found!", args[1]);
                            Console.WriteLine("Extract id {0}", args[2]);
                            string dic = Environment.CurrentDirectory + "\\" + args[1];
                            Extract(int.Parse(args[2]), int.Parse(args[3]), dic);
                            Console.WriteLine("Done!");
                            Console.ReadKey();
                        }
                        else
                        {
                            if (!File.Exists(Environment.CurrentDirectory + "\\" + args[2]))
                            {
                                Console.WriteLine("File {0} not exist!", args[1]);
                                Console.ReadKey();
                            }
                            else
                            {
                                string dic = Environment.CurrentDirectory + "\\" + args[1];
                                string dic2 = Environment.CurrentDirectory + "\\" + args[2];
                                Console.WriteLine("File {0} found!", args[1]);
                                Repack(dic, dic2);
                                Console.ReadKey();
                            }
                        }
                    }
                    catch
                    {
                        Console.WriteLine("Null id or file error!");
                        Console.ReadKey();
                    }
                }
            }
        }

        public static void Extract(int id, int obj, string path)
        {
            BinaryReader rd = new BinaryReader(File.OpenRead(path));
            string header = Encoding.ASCII.GetString(rd.ReadBytes(20));
            if(header == "QUANTICDREAMTABINDEX")
            {
                int version = Big(rd.ReadBytes(4));
                if(version == 18)
                {
                    rd.ReadBytes(77);
                    int num = Big(rd.ReadBytes(4));
                    string date = DateTime.Now.ToString();
                    int f = 0;
                    var filesize = new StreamWriter(MD5(date).ToLower() + ".FileSizeTable");
                    filesize.WriteLine("version " + version.ToString());
                    for (int i = 0; i < num; i++)
                    {
                        int idfile = Big(rd.ReadBytes(4));
                        rd.ReadInt32();
                        int num_obj = Big(rd.ReadBytes(4));
                        long por = rd.BaseStream.Position;
                        int offset = Big(rd.ReadBytes(4));
                        int lenght = Big(rd.ReadBytes(4));
                        rd.ReadInt32();
                        int file = Big(rd.ReadBytes(4));
                        string bf = string.Empty;
                        if (file == 0)
                            bf = ".dat";
                        else
                            bf = ".d" + file.ToString("d2");
                        try
                        {
                            if(obj != 0)
                            {
                                if (idfile == id && num_obj == obj)
                                {
                                    var rdbig = new BinaryReader(new FileStream(Environment.CurrentDirectory + "\\" + Path.GetFileNameWithoutExtension(path) + bf, FileMode.Open, FileAccess.Read, FileShare.Read));
                                    rdbig.BaseStream.Seek(offset, SeekOrigin.Begin);
                                    byte[] dataseg = rdbig.ReadBytes(lenght);
                                    if (!Directory.Exists(Environment.CurrentDirectory + "\\" + Path.GetFileNameWithoutExtension(path) + "_exp"))
                                        Directory.CreateDirectory(Environment.CurrentDirectory + "\\" + Path.GetFileNameWithoutExtension(path) + "_exp");
                                    BinaryWriter wt = new BinaryWriter(new FileStream(Environment.CurrentDirectory + "\\" + Path.GetFileNameWithoutExtension(path) + "_exp" + "\\" + "0x" + offset.ToString("x8").ToUpper() + ".dat", FileMode.Create, FileAccess.Write, FileShare.ReadWrite));
                                    wt.Write(dataseg);
                                    wt.Close();
                                    GetText(dataseg, path, offset);
                                    rdbig.Close();
                                    Console.WriteLine("0x" + offset.ToString("x8").ToUpper());
                                    filesize.WriteLine(por.ToString() + " \\" + Path.GetFileNameWithoutExtension(path) + "_exp" + "\\" + "0x" + offset.ToString("x8").ToUpper() + ".dat");
                                }
                            }
                            else
                            {
                                if (idfile == id)
                                {
                                    var rdbig = new BinaryReader(new FileStream(Environment.CurrentDirectory + "\\" + Path.GetFileNameWithoutExtension(path) + bf, FileMode.Open, FileAccess.Read, FileShare.Read));
                                    rdbig.BaseStream.Seek(offset, SeekOrigin.Begin);
                                    byte[] dataseg = rdbig.ReadBytes(lenght);
                                    if (!Directory.Exists(Environment.CurrentDirectory + "\\" + Path.GetFileNameWithoutExtension(path) + "_exp"))
                                        Directory.CreateDirectory(Environment.CurrentDirectory + "\\" + Path.GetFileNameWithoutExtension(path) + "_exp");
                                    BinaryWriter wt = new BinaryWriter(new FileStream(Environment.CurrentDirectory + "\\" + Path.GetFileNameWithoutExtension(path) + "_exp" + "\\" + "0x" + offset.ToString("x8").ToUpper() + ".dat", FileMode.Create, FileAccess.Write, FileShare.ReadWrite));
                                    wt.Write(dataseg);
                                    wt.Close();
                                    GetText(dataseg, path, offset);
                                    rdbig.Close();
                                    Console.WriteLine("0x" + offset.ToString("x8").ToUpper());
                                    filesize.WriteLine(por.ToString() + " \\" + Path.GetFileNameWithoutExtension(path) + "_exp" + "\\" + "0x" + offset.ToString("x8").ToUpper() + ".dat");
                                }
                            }
                        }
                        catch
                        {
                            Console.WriteLine("Cannot read file. File .dat or d0 not exist!");
                            Console.ReadKey();
                        }
                        f = file;
                    }
                    filesize.WriteLine(Path.GetFileNameWithoutExtension(path) + " " + f.ToString());
                    filesize.Close();
                }
                else
                {
                    Console.WriteLine("Only support version 18.");
                    Console.ReadKey();
                }
            }
            else
            {
                Console.WriteLine("Cannot read file. File is not QUANTICDREAMTABINDEX!");
                Console.ReadKey();
            }
        }

        public static void Repack(string idx, string sizetable)
        {
            string path = Path.GetDirectoryName(sizetable);
            if (!File.Exists(idx + "_bk"))
            {
                File.Copy(idx, idx + "_bk");
                Console.WriteLine("Backup file idx!");
            }
            Dictionary<string, string> keyfile = new Dictionary<string, string>();
            string[] stable = File.ReadAllLines(sizetable);
            int version = 0;
            string bigfnew = string.Empty;
            int filenum = 0;
            foreach (string addrkey in stable)
            {
                var data = addrkey.Split(' ');
                if (data[0] == "version")
                {
                    version = int.Parse(data[1]);
                    Console.WriteLine("Version {0}", version);
                    if (version != 18)
                    {
                        Console.WriteLine("Only support version 18.");
                        //Console.ReadKey();
                        Environment.Exit(1);
                    }
                }
                else if (!long.TryParse(data[0], out _))
                {
                    bigfnew = path + "\\" + data[0] + ".d" + (int.Parse(data[1]) + 1).ToString("d2");
                    filenum = int.Parse(data[1]) + 1;
                }
                else
                {
                    keyfile.Add(data[0], data[1]);
                }
            }
            BinaryWriter bigfile = new BinaryWriter(new FileStream(bigfnew, FileMode.Create, FileAccess.Write, FileShare.Read));
            bigfile.Write(Encoding.ASCII.GetBytes("QUANTICDREAMTABINDEX"));
            bigfile.Write(301989888);
            for (int i = 0; i < 2048; i++)
            {
                if (bigfile.BaseStream.Position == 2048)
                {
                    break;
                }
                else
                {
                    bigfile.Write((byte)0x2D);
                }
            }

            if (version == 18)
            {
                foreach (var pair in keyfile)
                {
                    if (!File.Exists(path + pair.Value + "_bk"))
                    {
                        File.Copy(path + pair.Value, path + pair.Value + "_bk");
                        Console.WriteLine("Backup file done!");
                    }
                    if (File.Exists(Path.GetDirectoryName(path + pair.Value) + "\\" + Path.GetFileNameWithoutExtension(path + pair.Value) + ".txt"))
                    {
                        Console.WriteLine("Load file {0}", Path.GetFileNameWithoutExtension(path + pair.Value) + ".txt");
                        using (var wt = new BinaryWriter(new FileStream(path + pair.Value, FileMode.Create, FileAccess.Write, FileShare.ReadWrite)))
                        {
                            using (var rd = new BinaryReader(new FileStream(path + pair.Value + "_bk", FileMode.Open, FileAccess.Read, FileShare.Read)))
                            {
                                Dictionary<string, string> textline = new Dictionary<string, string>();
                                string[] text = File.ReadAllLines(Path.GetDirectoryName(path + pair.Value) + "\\" + Path.GetFileNameWithoutExtension(path + pair.Value) + ".txt");
                                if (text.Length != 0)
                                {
                                    foreach (string line in text)
                                    {
                                        var dataline = line.Split('=');
                                        textline.Add(dataline[0], dataline[1]);
                                    }
                                    wt.Write(rd.ReadBytes(12));
                                    int tobe = rd.ReadInt32();
                                    byte[] fnum = rd.ReadBytes(tobe);
                                    wt.Write(tobe);
                                    wt.Write(fnum);
                                    wt.Write(rd.ReadBytes(24));
                                    //long porlen = wt.BaseStream.Position; //lengpor
                                    int chk = rd.ReadInt32();
                                    int lenb = rd.ReadInt32();
                                    int wnum = 0;
                                    byte[] datasheet = null;
                                    if (chk == 5)
                                    {
                                        rd.ReadByte();
                                        wnum = rd.ReadInt32();
                                        datasheet = rd.ReadBytes(lenb - 5);
                                    }
                                    else
                                    {
                                        wnum = rd.ReadInt32();
                                        datasheet = rd.ReadBytes(lenb - 4);
                                    }
                                    BinaryReader msrdm = new BinaryReader(new MemoryStream(datasheet));
                                    BinaryWriter tmp = new BinaryWriter(new FileStream("dump.tmp", FileMode.Create, FileAccess.ReadWrite, FileShare.ReadWrite));
                                    for (int i = 0; i < wnum; i++)
                                    {
                                        var dumpfs = new BinaryWriter(new FileStream(i + ".tmp", FileMode.Create, FileAccess.ReadWrite, FileShare.ReadWrite));
                                        dumpfs.Write(msrdm.ReadBytes(4));
                                        int heng = msrdm.ReadInt32();
                                        dumpfs.Write(heng);
                                        if (heng == 1196311808)
                                        {
                                            int count = msrdm.ReadInt32();
                                            dumpfs.Write(count);
                                            bool flagdata = false;
                                            int pl = 0;
                                            for (int j = 0; j < count; j++)
                                            {
                                                int lenid = msrdm.ReadInt32();
                                                dumpfs.Write(lenid);
                                                byte[] idsubb = msrdm.ReadBytes(lenid);
                                                string idsub = Encoding.ASCII.GetString(idsubb);
                                                dumpfs.Write(idsubb);
                                                lenid = msrdm.ReadInt32();
                                                msrdm.ReadBytes(lenid);
                                                string sub = textline[idsub];
                                                StringBuilder builder = new StringBuilder(sub);
                                                builder.Replace("[rn]", "\r\n");
                                                builder.Replace("[nr]", "\n\r");
                                                builder.Replace("[r]", "\r");
                                                builder.Replace("[n]", "\n");
                                                builder.Replace("[p]", "=");
                                                if (sub != "0")
                                                {
                                                    byte[] chargesub = Encoding.Unicode.GetBytes(builder.ToString());
                                                    dumpfs.Write(chargesub.Length);
                                                    dumpfs.Write(chargesub);
                                                }
                                                else
                                                {
                                                    dumpfs.Write(0);
                                                    //msrdm.ReadInt32();
                                                }
                                                int ckmk = msrdm.ReadInt32();
                                                dumpfs.Write(ckmk);
                                                if (ckmk == 1)
                                                {
                                                    flagdata = true;
                                                    pl++;
                                                }
                                            }
                                            if (flagdata == true)
                                            {
                                                int pnum = msrdm.ReadInt32();
                                                dumpfs.Write(pnum);
                                                if (pnum == count || pnum == pl)
                                                {
                                                    for (int g = 0; g < pnum; g++)
                                                    {
                                                        int nm = msrdm.ReadInt32();
                                                        dumpfs.Write(nm);
                                                        dumpfs.Write(msrdm.ReadBytes(nm));
                                                        byte cck = msrdm.ReadByte();
                                                        dumpfs.Write(cck);
                                                        if (cck == 1 && g == pnum - 1)
                                                        {
                                                            dumpfs.Write(msrdm.ReadBytes(5));
                                                        }
                                                        else if (cck == 1)
                                                        {
                                                            dumpfs.Write(msrdm.ReadBytes(9));
                                                        }
                                                        else
                                                        {
                                                            goto ENDCK;
                                                        }
                                                    }
                                                }
                                                else
                                                {
                                                    goto ENDCK;
                                                }

                                            }
                                        }
                                        else
                                        {
                                            int count = msrdm.ReadInt32();
                                            dumpfs.Write(count);
                                            bool flagdata = false;
                                            int pl = 0;
                                            for (int j = 0; j < count; j++)
                                            {
                                                int lenid = msrdm.ReadInt32();
                                                dumpfs.Write(lenid);
                                                dumpfs.Write(msrdm.ReadBytes(lenid));
                                                lenid = msrdm.ReadInt32();
                                                if (lenid != 0)
                                                {
                                                    dumpfs.Write(lenid);
                                                    dumpfs.Write(msrdm.ReadBytes(lenid));
                                                }
                                                else
                                                {
                                                    dumpfs.Write(lenid);
                                                }
                                                int ckmk = msrdm.ReadInt32();
                                                dumpfs.Write(ckmk);
                                                if (ckmk == 1)
                                                {
                                                    flagdata = true;
                                                    pl++;
                                                }
                                            }
                                            if (flagdata == true)
                                            {
                                                int pnum = msrdm.ReadInt32();
                                                dumpfs.Write(pnum);
                                                if (pnum == count || pnum == pl)
                                                {
                                                    for (int g = 0; g < pnum; g++)
                                                    {
                                                        int nm = msrdm.ReadInt32();
                                                        dumpfs.Write(nm);
                                                        dumpfs.Write(msrdm.ReadBytes(nm));
                                                        byte cck = msrdm.ReadByte();
                                                        dumpfs.Write(cck);
                                                        if (cck == 1 && g == pnum - 1)
                                                        {
                                                            dumpfs.Write(msrdm.ReadBytes(5));
                                                        }
                                                        else if (cck == 1)
                                                        {
                                                            dumpfs.Write(msrdm.ReadBytes(9));
                                                        }
                                                        else if (cck == 0 && g == pnum - 1)
                                                        {
                                                            goto ENDCK;
                                                        }
                                                    }
                                                }
                                                else
                                                {
                                                    goto ENDCK;
                                                }

                                            }
                                        }
                                        dumpfs.Write(msrdm.ReadBytes(4));
                                    ENDCK:;
                                        dumpfs.Close();
                                        byte[] dumpall = File.ReadAllBytes(i + ".tmp");
                                        tmp.Write(dumpall);
                                        dumpfs.Close();
                                        File.Delete(i + ".tmp");
                                    }
                                    tmp.Close();
                                    byte[] ftmp = File.ReadAllBytes("dump.tmp");
                                    wt.Write(chk);
                                    if (chk == 5)
                                    {
                                        wt.Write(ftmp.Length + 5);
                                        wt.Write((byte)0x00);
                                        wt.Write(wnum);
                                    }
                                    else
                                    {
                                        wt.Write(ftmp.Length + 4);
                                        wt.Write(wnum);
                                    }
                                    wt.Write(ftmp);
                                    wt.Flush();
                                    wt.Close();
                                    File.Delete("dump.tmp");
                                }
                            }
                        }
                        Console.WriteLine("Import file {0} done!", Path.GetFileName(path + pair.Value));
                    }

                    for (int i = 0; i < 64; i++)
                    {
                        if (bigfile.BaseStream.Position % 64 != 0)
                        {
                            bigfile.Write((byte)0x2D);
                        }
                        else
                        {
                            byte[] dcsegs = File.ReadAllBytes(path + pair.Value);
                            using (var idmapwt = new BinaryWriter(new FileStream(idx, FileMode.Open, FileAccess.Write, FileShare.ReadWrite)))
                            {
                                long addm = long.Parse(pair.Key);
                                idmapwt.BaseStream.Seek(addm, SeekOrigin.Begin);
                                byte[] porar = BitConverter.GetBytes((int)bigfile.BaseStream.Position);
                                Array.Reverse(porar);
                                idmapwt.Write(porar);
                                byte[] lenar = BitConverter.GetBytes((int)dcsegs.Length);
                                Array.Reverse(lenar);
                                idmapwt.Write(lenar);
                                idmapwt.BaseStream.Seek(4, SeekOrigin.Current);
                                byte[] flar = BitConverter.GetBytes((int)filenum);
                                Array.Reverse(flar);
                                idmapwt.Write(flar);
                            }
                            bigfile.Write(dcsegs);
                            break;
                        }
                    }
                }
            }
            bigfile.Flush();
            bigfile.Close();
        }

        public static int Big(byte[] input)
        {
            Array.Reverse(input);
            return BitConverter.ToInt32(input, 0);
        }

        public static string MD5(string input)
        {
            using (System.Security.Cryptography.MD5 md5 = System.Security.Cryptography.MD5.Create())
            {
                byte[] inputBytes = System.Text.Encoding.ASCII.GetBytes(input);
                byte[] hashBytes = md5.ComputeHash(inputBytes);
                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < hashBytes.Length; i++)
                {
                    sb.Append(hashBytes[i].ToString("X2"));
                }
                return sb.ToString();
            }
        }

        public static void GetText(byte[] datatext , string path, int offset)
        {
            using (var adatarb = new BinaryReader(new MemoryStream(datatext)))
            {
                List<string> lstxt = new List<string>();
                bool flags = false;
                Int64 hadatarb = adatarb.ReadInt64();
                if (hadatarb == 6074880098149683011)
                {
                    adatarb.ReadInt32();
                    int lnum = adatarb.ReadInt32();
                    adatarb.ReadBytes(lnum);
                    adatarb.ReadBytes(16);
                    Int64 hlocal = adatarb.ReadInt64();
                    if (hlocal == 6870884773368385356)
                    {
                        flags = true;
                        int ckk = adatarb.ReadInt32();
                        int lb = adatarb.ReadInt32();
                        if (ckk == 5)
                            adatarb.ReadByte();
                        int cnum = adatarb.ReadInt32();
                        for (int c = 0; c < cnum; c++)
                        {
                            adatarb.ReadBytes(4);
                            int heng = adatarb.ReadInt32();
                            if (heng == 1196311808)
                            {
                                int knum = adatarb.ReadInt32();
                                bool flagdata = false;
                                int pl = 0;
                                for (int g = 0; g < knum; g++)
                                {
                                    int count = adatarb.ReadInt32();//null
                                    byte[] txtid = adatarb.ReadBytes(count);
                                    count = adatarb.ReadInt32();
                                    byte[] txtout = adatarb.ReadBytes(count);
                                    string converted = Encoding.Unicode.GetString(txtout);
                                    StringBuilder builder = new StringBuilder(converted);
                                    builder.Replace("\r\n", "[rn]");
                                    builder.Replace("\n\r", "[nr]");
                                    builder.Replace("\r", "[r]");
                                    builder.Replace("\n", "[n]");
                                    builder.Replace("=", "[p]");
                                    if (builder.Length == 0)
                                        lstxt.Add(Encoding.ASCII.GetString(txtid) + "=0");
                                    else
                                        lstxt.Add(Encoding.ASCII.GetString(txtid) + "=" + builder.ToString());
                                    if (adatarb.ReadInt32() == 1)
                                    {
                                        flagdata = true;
                                        pl++;
                                    }
                                }
                                if (flagdata == true)
                                {
                                    int num = adatarb.ReadInt32();
                                    if (num == knum || num == pl)
                                    {
                                        for (int g = 0; g < num; g++)
                                        {
                                            int count = adatarb.ReadInt32();
                                            adatarb.ReadBytes(count);
                                            byte ckck = adatarb.ReadByte();
                                            if (ckck == 1 && g == num - 1)
                                            {
                                                adatarb.ReadBytes(5);
                                            }
                                            else if (ckck == 1)
                                            {
                                                adatarb.ReadBytes(9);
                                            }
                                            else if (ckck == 0 && g == num - 1)
                                            {
                                                goto END;
                                            }
                                        }
                                    }
                                    else
                                    {
                                        goto END;
                                    }
                                }
                            }
                            else
                            {
                                int knum = adatarb.ReadInt32();
                                bool flagdata = false;
                                int pl = 0;
                                for (int g = 0; g < knum; g++)
                                {
                                    int count = adatarb.ReadInt32();//null
                                    adatarb.ReadBytes(count);
                                    count = adatarb.ReadInt32();
                                    adatarb.ReadBytes(count);
                                    if (adatarb.ReadInt32() == 1)
                                    {
                                        flagdata = true;
                                        pl++;
                                    }
                                }
                                if (flagdata == true)
                                {
                                    int num = adatarb.ReadInt32();
                                    if (num == knum || num == pl)
                                    {
                                        for (int g = 0; g < num; g++)
                                        {
                                            int count = adatarb.ReadInt32();
                                            adatarb.ReadBytes(count);
                                            byte ckck = adatarb.ReadByte();
                                            if (ckck == 1 && g == (num - 1))
                                            {
                                                adatarb.ReadBytes(5);
                                            }
                                            else if (ckck == 1)
                                            {
                                                adatarb.ReadBytes(9);
                                            }
                                            else if (ckck == 0 && g == num - 1)
                                            {
                                                goto END;
                                            }
                                        }
                                    }
                                    else
                                    {
                                        goto END;
                                    }
                                }
                            }
                            adatarb.ReadBytes(4);
                        END:;
                        }
                    }
                }
                if (flags)
                {
                    using (StreamWriter wttxt = new StreamWriter(Environment.CurrentDirectory + "\\" + Path.GetFileNameWithoutExtension(path) + "_exp" + "\\" + "0x" + offset.ToString("x8").ToUpper() + ".txt"))
                    {
                        foreach (String s in lstxt)
                            wttxt.WriteLine(s);
                    }
                    Console.WriteLine("Done!");
                }
                else
                {
                    Console.WriteLine("Break!");
                }
            }
        }
    }
}
