﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections;
using System.IO;
using System.Text.RegularExpressions;
using MIPS246.Core.DataStructure;


namespace MIPS246.Core.Assembler
{
    public class Assembler
    {
        #region Fields
        private List<Instruction> codelist;
        private List<string[]> sourceList;
        private AssemblerErrorInfo error;
        private string sourcepath;
        private uint address;
        private uint line;
        private Hashtable addresstable;
        private bool foundadd0;

        //config
        private static uint startAddress = 0;
        #endregion

        #region Constructors
        public Assembler(string sourcepath)
        {
            this.sourcepath = sourcepath;
            sourceList = new List<string[]>();
            codelist = new List<Instruction>();
            addresstable = new Hashtable();

            address = 0;
            line = 1;
            foundadd0 = false;
        }
        #endregion

        #region Properties
        public List<Instruction> CodeList
        {
            get
            {
                return this.codelist;
            }
        }
        #endregion

        #region Public Methods
        public bool DoAssemble()
        {
            if (this.LoadFile() == false)
            {
                this.error = new AssemblerErrorInfo(0, AssemblerError.NOFILE);
                return false;
            }

            if (LoadAddress() == false)
            {
                return false;
            }


            return true;
        }

        public string Display(Instruction instruction)
        {
            return "0x"+String.Format("{0:X8}", instruction.Address)+":\t"+DisplayHexCMD(instruction.Machine_Code);
        }

        public string DisplayHexCMD(bool[] machine_code)
        {
            string machine_codeSTR = string.Empty;
            for (int i = 0; i < 8; i++)
            {
                machine_codeSTR = machine_codeSTR + InttoHex(8 * BoolToInt(machine_code[i * 4]) +
                    4 * BoolToInt(machine_code[i * 4 + 1]) +
                    2 * BoolToInt(machine_code[i * 4 + 2]) +
                    BoolToInt(machine_code[i * 4 + 3]));
            }

            return machine_codeSTR; 
        }

        public void DisplayError()
        {
            Console.WriteLine("Compile failed:");
            this.error.Display();
        }       
        #endregion

        #region Internal Methods      
        private bool LoadFile()
        {
            if (File.Exists(sourcepath) == false)
            {
                this.error = new AssemblerErrorInfo(0, AssemblerError.NOFILE);
                return false;
            }
            else
            {
                StreamReader sr = new StreamReader(sourcepath);
                string linetext;
                while ((linetext = sr.ReadLine()) != null)
                {
                    linetext = RemoveComment(linetext);
                    sourceList.Add(linetext.Split());
                }
                return true;
            }
        }

        private bool LoadAddress()
        {
            for (int i = 0; i < sourceList.Count; i++)
            {
                if(sourceList[i][0].EndsWith(":"))
                {
                    string label = sourceList[i][0].Substring(0, sourceList[i][0].Length - 1);
                    Regex reg = new Regex("^[a-zA-Z_][a-zA-Z0-9_]*");
                    if (reg.IsMatch(label) == false)
                    {
                        this.error = new AssemblerErrorInfo(i, AssemblerError.INVALIDLABEL, label);
                        return false;
                    }
                    addAddresstable(sourceList[i][0].Substring(0, sourceList[i][0].Length - 1), i);
                }
            }
            return true;
        }

        private bool CheckWord()
        {
            return true;
        }

        private void addAddresstable(string addressname, int address)
        {
            addresstable.Add(addressname, address);
        }

        private void CheckWord(string [] split)
        {
            switch (split.Length)
            {
                case 1:
                    CheckOneWord(split);
                    line++;
                    break;
                case 2:
                    CheckTwoWord(split);
                    line++;
                    break;
                case 3:
                    CheckThreeWord(split);
                    line++;
                    break;
                default:
                    //this.errorlist.Add(new AssemblerErrorInfo(line, AssemblerError.UNKNOWNCMD, "Line " + line + ": " + "Unknown command."));
                    line++;
                    break;
            }
        }

        private void CheckOneWord(string[] split)
        {
            switch (split[0])
            {
                case ".text":
                case ".data":
                case ".memory":
                    break;
                default:
                    //this.errorlist.Add(new AssemblerErrorInfo(line, AssemblerError.UNKNOWNCMD, "Line " + line + ": " + "Unknown command."));
                    break;
            }
        }

        private void CheckTwoWord(string [] split)
        {
            switch (split[0])
            {
                case ".globl":
                    if (foundadd0 == false)
                    {
                        address = 0;
                        //addAddresstable(split[1], address);
                        foundadd0 = true;
                        break;
                    }
                    else
                    {
                        //this.errorlist.Add(new AssemblerErrorInfo(line, AssemblerError.TWOADD0, "Line " + line + ": " + "Address 0 has been defined."));
                        break;
                    }
                case "JR":
                case "JALR":                
                    if (CheckRegister(split[1]) == true)
                    {
                        address += 4;
                        Instruction instruction = new Instruction(split[0], split[1], string.Empty, string.Empty, address);                        
                        instruction.Validate();                        
                        codelist.Add(instruction);                        
                        break;
                    }
                    else
                    {
                       // this.errorlist.Add(new AssemblerErrorInfo(line, AssemblerError.WRONGREGNAME, "Line " + line + ": " + "Wrong register name:"+split[1]));
                        break;
                    }
                case "J":
                case "JAL":
                    if (CheckAddress(split[1]))
                    {
                        address += 4;
                        Instruction instruction = new Instruction(split[0], ConvertAddress(split[1]), string.Empty, string.Empty, address);
                        instruction.Validate();                        
                        codelist.Add(instruction);
                        
                        break;
                    }
                    else
                    {
                        //this.errorlist.Add(new AssemblerErrorInfo(line, AssemblerError.UNKNOWNADDLABEL, "Line " + line + ": " + "The address label is not define: " + split[1]));
                        line++;
                        break;
                    }
                default:
                    //this.errorlist.Add(new AssemblerErrorInfo(line, AssemblerError.UNKNOWNCMD, "Line " + line + ": " + "Unknown command."));
                    line++;
                    break;
            }
        }

        private void CheckThreeWord(string[] split)
        {

        }

        private bool CheckRegister(string reg)
        {
            switch (reg)
            {
                case "$0":
                case "$zero":
                case "$1":
                case "$at":
                case "$2":
                case "$v0":
                case "$3":
                case "$v1":
                case "$4":
                case "$a0":
                case "$5":
                case "$a1":
                case "$6":
                case "$a2":
                case "$7":
                case "$a3":
                case "$8":
                case "$t0":
                case "$9":
                case "$t1":
                case "$10":
                case "$t2":
                case "$11":
                case "$t3":
                case "$12":
                case "$t4":
                case "$13":
                case "$t5":
                case "$14":
                case "$t6":
                case "$15":
                case "$t7":
                case "$16":
                case "$s0":
                case "$17":
                case "$s1":
                case "$18":
                case "$s2":
                case "$19":
                case "$s3":
                case "$20":
                case "$s4":
                case "$21":
                case "$s5":
                case "$22":
                case "$s6":
                case "$23":
                case "$s7":
                case "$24":
                case "$t8":
                case "$25":
                case "$t9":
                case "$26":
                case "$k0":
                case "$27":
                case "$k1":
                case "$28":
                case "$gp":
                case "$29":
                case "$sp":
                case "$30":
                case "$fp":
                case "$31":
                case "$ra":
                    return true;
                default:
                    return false;
            }
        }

        private bool CheckAddress(string addressname)
        {
            return this.addresstable.Contains(addressname);
        }

        private string ConvertAddress(string addressname)
        {
            return this.addresstable[addressname].ToString();
        }

        private string RemoveComment(string str)
        {
           if (str.Contains('#'))
           {
               if (str.IndexOf('#') != 0)
               {
                   str = str.Substring(0, str.IndexOf("#") - 1);
               }
               else
               {
                   str = string.Empty;
               }
           }
           return str;
        }
        
         private string InttoHex(int i)
        {
            switch (i)
            {
                case 1:
                case 2:
                case 3:
                case 4:
                case 5:
                case 6:
                case 7:
                case 8:
                case 9:
                    return i.ToString();
                case 11:
                    return "A";
                case 12:
                    return "B";
                case 13:
                    return "C";
                case 14:
                    return "D";
                case 15:
                    return "E";
                case 16:
                    return "F";
                default:
                    return i.ToString();
            }
        }

        private int BoolToInt(bool bit)
        {
            switch (bit)
            {
                case true:
                    return 1;
                case false:
                    return 0;
                default:
                    return 0;
            }
        }
        #endregion

        #region OPs

        #endregion
    }
}
