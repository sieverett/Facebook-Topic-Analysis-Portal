import React, { Component } from 'react';
import PagedDataTableBar from './PagedDataTableBar';

class DataTable extends Component {
  static get defaultProps() {
    return {
      className: '',
      alwaysShowPaginationForm: true,
      showHeader: true,
      showIndex: true,
      striped: true,
      mapping: [],
      data: []
    };
  }

  title = () => this.props.title ? <h2>{this.props.title}</h2> : undefined;

  handlePageNumberChanged = (pageNumber) => this.props.onPaginationChanged(pageNumber, this.props.pagination.pageSize);
  handlePageSizeChanged = (pageSize) => this.props.onPaginationChanged(this.props.pagination.pageNumber, pageSize);

  paginationBar = () => {
    // The user can optionally specify pagination.
    if (!this.props.pagination) {
      return undefined;
    }

    // The user can hide the pagination form if we don't actually need it.
    var endIndex = this.props.data ? this.props.data.length : 0;
    if (!this.props.alwaysShowPaginationForm) {
      if (!endIndex || endIndex < this.props.pagination.pageSize) {
        return undefined;
      }
    }

    return <PagedDataTableBar {...this.props.pagination}
                              onPageNumberChanged={this.handlePageNumberChanged}
                              onPageSizeChanged={this.handlePageSizeChanged} />;
  }

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
            return <th key={row.name || row}>{row.name || row}</th>;
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
    // The user can provide a start index. If none is provided, we fall back to the pagination
    // index.
    var startIndex = this.props.startIndex;
    if (!startIndex && this.props.pagination && this.props.pagination.pageSize) {
      startIndex = (this.props.pagination.pageNumber - 1) * this.props.pagination.pageSize + 1;
    }
    if (!startIndex) {
      startIndex = 0;
    }

    // Loop through the data and create rows.
    var rows = this.props.data.map((data, index) => {
      // The user can provide a handler that is triggered each time a row is clicked.
      var selectableClass = this.props.onRowSelected ? 'selectable-row' : '';
      var handleOnClick = () => {
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
        <tr key={index} className={selectableClass} onClick={handleOnClick}>
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

    // The user can provide a minimum size. If none is provided, we fall back to the pagination
    // size.
    var paginationSize = this.props.pagination ? this.props.pagination.pageSize : undefined;
    var minPageSize = this.props.minSize || paginationSize;

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

    // Render the title, pagination bar, column names and then the rows.
    return (
      <section className="data-table">
        {this.title()}
        <div className="table-responsive">
          {this.paginationBar()}
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
