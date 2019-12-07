using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GRPExplorerLib.Logging;
using GRPExplorerLib.Util;
using System.IO;
using GRPExplorerLib.BigFile;

namespace GRPExplorerLib.YetiObjects
{
    public enum DataTableColumnType
    {
        Int = 0x01,
        UNK01 = 0x02,
        String = 0x03,
        AssetKey = 0x04,
    }

    public abstract class DataTableRow
    {
        public class IntRow : DataTableRow
        {
            private int value;

            public override void ReadValue(Stream stream)
            {
                BinaryReader r = new BinaryReader(stream);
                value = r.ReadInt32();
            }

            public override DataTableRow Copy()
            {
                return new IntRow { value = this.value };
            }

            public override string ToString()
            {
                return value.ToString();
            }
        }

        public class StringRow : DataTableRow
        {
            private string value = "";

            public override void ReadValue(Stream stream)
            {
                value = StringUtil.ReadNullTerminatedString(stream);
            }

            public override DataTableRow Copy()
            {
                return new StringRow { value = this.value };
            }

            public override string ToString()
            {
                return value;
            }
        }

        public class AssetKeyRow : DataTableRow
        {
            private int key;

            public override void ReadValue(Stream stream)
            {
                BinaryReader r = new BinaryReader(stream);
                key = r.ReadInt32();
            }

            public override DataTableRow Copy()
            {
                return new AssetKeyRow { key = this.key };
            }

            public override string ToString()
            {
                return string.Format("{0:X8}", key);
            }
        }

        public abstract void ReadValue(Stream stream);
        public abstract DataTableRow Copy();
    }

    public class DataTableColumn
    {
        public string ColumnName { get; private set; }
        public DataTableColumnType Type { get; private set; }
        public DataTableRow DefaultRow { get; private set; }
        public DataTableRow[] Rows { get; private set; }

        public DataTableRow this[int index] => Rows[index];

        public DataTableColumn(string name, DataTableColumnType type, int rowHeight)
        {
            ColumnName = name;
            Type = type;
            Rows = new DataTableRow[rowHeight];
        }

        public void SetDefault(Stream stream)
        {
            switch (Type)
            {
                case DataTableColumnType.Int:
                    DefaultRow = new DataTableRow.IntRow();
                    break;
                case DataTableColumnType.String:
                    DefaultRow = new DataTableRow.StringRow();
                    break;
                case DataTableColumnType.AssetKey:
                case DataTableColumnType.UNK01: //use assetkeyrow to display the value for this kind of parameter in hexadecimal
                    DefaultRow = new DataTableRow.AssetKeyRow();
                    break;
                default:
                    throw new Exception("COLUMN TYPE NOT FOUND");
            }

            DefaultRow.ReadValue(stream);

            for (int i = 0; i < Rows.Length; i++)
                Rows[i] = DefaultRow.Copy();
        }
    }

    public class DataTable : YetiObjectArchetype
    {
        public override YetiObjectType Identifier => YetiObjectType.dtb;

        public int NumColumns { get; private set; }
        public int NumRows { get; private set; }

        public DataTableColumn[] Columns { get; private set; }
        public DataTableColumn this[int index] => Columns[index]; 

        public override void Load(byte[] buffer, int size, BigFileFile[] fileReferences)
        {
            NumColumns = BitConverter.ToInt32(buffer, 0);
            NumRows = BitConverter.ToInt32(buffer, 4);

            Columns = new DataTableColumn[NumColumns];

            using (MemoryStream ms = new MemoryStream(buffer))
            using (BinaryReader br = new BinaryReader(ms))
            {
                ms.Seek(8, SeekOrigin.Begin);

                string colName = "";
                DataTableColumnType colType;

                for (int i = 0; i < NumColumns; i++)
                {
                    colName = StringUtil.ReadNullTerminatedString(ms);
                    colType = (DataTableColumnType)br.ReadInt32();
                    Columns[i] = new DataTableColumn(colName, colType, NumRows);
                    Columns[i].SetDefault(ms);
                }

                for (int i = 0; i < NumRows; i++)
                {
                    for (int j = 0; j < NumColumns; j++)
                    {
                        this[j][i].ReadValue(ms);
                    }
                }
            }
        }

        public override void Log(ILogProxy log)
        {
            log.Info("DATATABLE NumRows: {0}  NumColumns: {1}", NumRows, NumColumns);
            StringBuilder builder1 = new StringBuilder();
            for (int i = 0; i < NumColumns; i++)
            {
                builder1.Append(this[i].ColumnName).Append(" | ");
            }
            log.Info("   {0}", builder1.ToString());

            for (int i = 0; i < NumRows; i++)
            {
                StringBuilder builder = new StringBuilder();
                for (int j = 0; j < NumColumns; j++)
                {
                    builder.Append(this[j][i]).Append(" | ");
                }
                log.Info("   {0}", builder.ToString());
            }
        }
    }
}
