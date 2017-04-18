import React, { Component } from 'react';
import PageControl from './PageControl';
import NumericValueForm from '../NumericValueForm';

class PagedDataTableBar extends Component {
  static get defaultProps() {
    return {
      pageNumber: 1,
      pageSize: 50,
      numberOfPages: 1,
      showPageNumberForm: true,
      showPageSizeForm: true
    };
  }

  render() {
    // The page indicators bar is always visible, but the user can show or hide the forms
    // for changing the current page number or the current page size.
    return (
      <nav className="form-inline">
        <PageControl {...this.props} onPageChanged={this.props.onPageNumberChanged} />

        {this.props.showPageNumberForm &&
          <NumericValueForm name="Page Number"
              placeholder="Page Number"
              action="Go To Page"
              value={this.props.pageNumber}
              minValue={1}
              maxValue={this.props.numberOfPages}
              onSubmit={this.props.onPageNumberChanged}
          />
          }

        {this.props.showPageSizeForm &&
          <NumericValueForm name="Page Size"
              placeholder="Page Size"
              action="Change Page Size"
              value={this.props.pageSize}
              minValue={1}
              onSubmit={this.props.onPageSizeChanged}
          />
        }
      </nav>
    );
  }
}

export default PagedDataTableBar;
