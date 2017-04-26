import React, { Component } from 'react';

class DataTable extends Component {
  static get defaultProps() {
    return {
      className: '',
      showHeader: true,
      showIndex: true,
      striped: true,
      mapping: [],
      data: []
    };
  }

  title = () => this.props.title ? <h2>{this.props.title}</h2> : undefined;

  header = () => {
    // The user can optionally hide the header.
    if (!this.props.showHeader) {
      return undefined;
    }

    // We need to add extra columns if we're showing the index on the left or the
    // selection indicator on the right.
    return (
      <thead>
        <tr>
          {this.props.showIndex &&
            <th>#</th>
          }
          {this.props.mapping.map(row => {
            const value = row.name || row;

            // The user can allow this column to be ordered by providing the field 'orderingKey'
            // in the mapping.
            if (row.orderingKey) {
              const handleClicked = (e) => {
                e.preventDefault();
                
                let descendingOrder;
                if (row.orderingKey === this.props.orderingKey) {
                  // If this row is currently selected, toggle it's ordering type.
                  descendingOrder = !this.props.orderDescending;
                } else {
                  // Default to descending order.
                  descendingOrder = true;
                }

                this.props.onOrderingChanged(row.orderingKey, descendingOrder);
              };

              // Emphasize the column if it is column we are currently using to order.
              let extra;
              if (row.orderingKey === this.props.orderingKey) {
                // Show the correct display depending on if we are in ascending or descending order.
                if (this.props.orderDescending) {
                  extra = '▼';
                } else {
                  extra = '▲';
                }
              } 

              return <th key={value}><a href="#" onClick={handleClicked}>{value}{extra}</a></th>
            } else {
              return <th key={value}>{value}</th>;
            }
          })}
          {this.props.selectionChecker &&
            <th />
          }
        </tr>
      </thead>
    );
  }

  getValue = (row, data) => {
    // The user can pass a key selector, e.g. [{name: 'Name1', key: ...}]
    if (row.key) {
      // The user can pass a key function, e.g. [{name: 'Name1', key: d => d.otherName}]
      if (typeof row.key === 'function') {
        var returnValueKeyFunction = row.key(data);
        if (returnValueKeyFunction !== undefined) {
          return returnValueKeyFunction;
        }
      }

      // The user can pass a key identifier, e.g. [{name: 'Name1', key: 'otherName'}]
      var rowKeyString = String(row.key);
      if (rowKeyString) {
        var returnValueKeyIndexer = data[rowKeyString.toLowerCase().trim()];
        if (returnValueKeyIndexer !== undefined) {
          return returnValueKeyIndexer;
        }
      }
    }

    // The user can pass a name: e.g. [{name: 'Name1'}, {name: 'Name2'}]
    if (row.name) {
      var returnValueNameIndexer = data[row.name.toLowerCase().trim()];
      if (returnValueNameIndexer !== undefined) {
        return returnValueNameIndexer;
      }
    }

    // The user can pass a name directly, e.g. ['Name1', 'Name2'],
    var rowString = String(row);
    if (rowString) {
      var returnValueStringIndexer = data[rowString.toLowerCase().trim()];
      if (returnValueStringIndexer !== undefined) {
        return returnValueStringIndexer;
      }
    }

    // If we can't resolve it, try converting the current data object to a string
    return String(data) || 'No Data';
  }

  rows = () => {
    // The user can provide a start index.
    const startIndex = this.props.startIndex || 0;

    // Loop through the data and create rows.
    var rows = this.props.data.map((data, index) => {
      // The user can provide a handler that is triggered each time a row is clicked.
      var selectableClass = this.props.onRowSelected ? 'selectable-row' : '';
      var handleRowClicked = () => {
        if (this.props.onRowSelected) {
          this.props.onRowSelected(data, index);
        }
      };

      // The user can provide information as to whether a row is selected or not.
      var selectedCell;
      if (this.props.selectionChecker) {
        if (this.props.selectionChecker(data, index)) {
          selectedCell = <td><span className="selected-cell">✓</span></td>;
        } else {
          selectedCell = <td />;
        }
      }

      // We need to add extra cells if we're showing the index on the left or the
      // selection indicator on the right.
      return (
        <tr key={index} className={selectableClass} onClick={handleRowClicked}>
          {this.props.showIndex &&
            <td>{index + startIndex}</td>
          }
          {this.props.mapping.map((row, index) => {
            return <td key={row.key || index}>{this.getValue(row, data)}</td>;
          })}
          {selectedCell}
        </tr>
      );
    });

    // The user can provide a minimum size.
    const minPageSize = this.props.minSize;

    // We need to add extra cells if we're showing the index on the left or the
    // selection indicator on the right.
    if (minPageSize && rows.length < minPageSize) {
      for (var i = rows.length; i < minPageSize; i++) {
        rows.push(<tr key={i} className='blank-row'>
          {this.props.showIndex &&
            <td />
          }
          {this.props.mapping.map((row, index) => {
            return <td key={row.key || index} />;
          })}
          {this.props.selectionChecker &&
            <td />
          }
          </tr>);
      }
    }

    return rows;
  }

  render() {
    // The user can customize whether the table is striped or not.
    var tableClassName = 'table';
    if (this.props.striped) {
      tableClassName += ' table-striped';
    }

    // The user can pass additional styles to the table.
    if (this.props.className) {
      tableClassName += ` ${this.props.className}`;
    }

    // Render the title, column names and then the rows.
    return (
      <section className="data-table">
        {this.title()}
        <div className="table-responsive">
          <table className={tableClassName}>
            {this.header()}
            <tbody>
              {this.rows()}
            </tbody>
          </table>
        </div>
      </section>
    );
  }
}

export default DataTable;
