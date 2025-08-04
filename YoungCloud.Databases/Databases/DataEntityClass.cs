using System;
using System.Data;
using System.Globalization;

namespace YoungCloud.Databases
{
    /// <summary>
    /// Data entity base class.
    /// </summary>
    [Serializable]
    public abstract class DataEntityClass : Disposable, IDataEntityClass
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DataEntityClass">DataEntityClass</see> class.
        /// </summary>
        protected DataEntityClass()
        {
            Container = new DataSet();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DataEntityClass">DataEntityClass</see> class.
        /// </summary>
        /// <param name="row">Datarow object with data.</param>
        protected DataEntityClass(DataRow row)
        {
            Container = new DataSet();
            SetRowValue(row);
        }

        private DataSet Container
        {
            get;
            set;
        }

        /// <summary>
        /// Create enpty DataRow instance with columns.
        /// </summary>
        /// <param name="dataColumnCol">The collection whith columns.</param>
        /// <returns>DataRow instance.</returns>
        protected DataRow CreateDataRow(params DataColumn[] dataColumnCol)
        {
            using (DataTable _DataTable = new DataTable())
            {
                _DataTable.Locale = CultureInfo.InvariantCulture;
                _DataTable.Columns.AddRange(dataColumnCol);
                Container.Tables.Clear();
                Container.Tables.Add(_DataTable);
                return _DataTable.NewRow();
            }
        }

        /// <summary>
        /// System.Data.DataRow.
        /// </summary>
        public DataRow DataRow
        {
            get
            {
                return Container.Tables[0].Rows[0];
            }
            set
            {
                SetRowValue(value);
            }
        }

        /// <summary>
        /// Release all resources.
        /// </summary>
        /// <param name="disposing">Is invoked from Dispose method or not.</param>
        protected override void Dispose(bool disposing)
        {
            if (!Disposed)
            {
                if (disposing)
                {
                    Container.Dispose();
                }
            }
            base.Dispose(disposing);
        }

        /// <summary>
        /// Get the value of column.
        /// </summary>
        /// <param name="columnName">The name of culumn.</param>
        /// <returns>The value of culumn.</returns>
        /// <exception cref="DataRowNullReferenceException">This exception throws when the DataRow object is not exist.</exception>
        protected virtual object GetColumnValue(string columnName)
        {
            if (Container.Tables.Count == 0)
            {
                throw new DataRowNullReferenceException();
            }
            else if (Container.Tables[0].Rows.Count == 0)
            {
                throw new DataRowNullReferenceException();
            }
            else
            {
                return Container.Tables[0].Rows[0][columnName];
            }
        }
        /// <summary>
        /// Set the value of column.
        /// </summary>
        /// <param name="columnName">The name of column.</param>
        /// <param name="columnValue">The value of column.</param>
        /// <exception cref="WrongDataTypeException">This exception throws when then data type is not the same as the column.</exception>
        /// <exception cref="DataRowNullReferenceException">This exception throws when the DataRow object is not exist.</exception>
        protected virtual void SetColumnValue(string columnName, object columnValue)
        {
            if (Container.Tables.Count == 0)
            {
                throw new DataRowNullReferenceException();
            }
            else if (Container.Tables[0].Rows.Count == 0)
            {
                throw new DataRowNullReferenceException();
            }
            else
            {
                Container.Tables[0].Rows[0].BeginEdit();
                Container.Tables[0].Rows[0][columnName] = columnValue;
                Container.Tables[0].Rows[0].EndEdit();
            }
        }

        /// <summary>
        /// Create data container and set values.
        /// </summary>
        /// <param name="row">Data instance.</param>
        private void SetRowValue(DataRow row)
        {
            using (DataTable _Table = new DataTable())
            {
                DataColumn[] _Columns = new DataColumn[row.Table.Columns.Count];
                for (int i = 0; i < row.Table.Columns.Count; i++)
                {
                    using (DataColumn _Column = new DataColumn())
                    {
                        _Column.ColumnName = row.Table.Columns[i].ColumnName;
                        _Column.DataType = row.Table.Columns[i].DataType;
                        _Columns[i] = _Column;
                    }
                }
                _Table.Columns.AddRange(_Columns);
                _Table.Rows.Add(row.ItemArray);
                Container.Tables.Clear();
                Container.Tables.Add(_Table);
            }
        }
    }
}