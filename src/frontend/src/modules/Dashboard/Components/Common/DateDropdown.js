import React, { Component } from 'react';

class DateDropdown extends Component {
  daysAgo = (numDaysAgo) => { 
    var date = new Date();
    date.setDate(date.getDate() - numDaysAgo);
    return date;
  }

  monthsAgo = (numMonthsAgo) => { 
    var date = new Date();
    date.setMonth(date.getMonth() - numMonthsAgo);
    return date;
  }

  handleDateChanged = (event) => {
    event.preventDefault();

    // Adjust the date for the user's selection.
    var date = null;
    var type = event.target.type;
    if (type === 'now') {
      date = new Date();
    } else if (type === 'one-day-ago') {
      date = this.daysAgo(1);
    } else if (type === 'two-days-ago') {
      date = this.daysAgo(1);
    } else if (type === 'one-week-ago') {
      date = this.daysAgo(7);
    } else if (type === 'two-weeks-ago') {
      date = this.daysAgo(14);
    } else if (type === 'three-weeks-ago') {
      date = this.daysAgo(21);
    } else if (type === 'one-month-ago') {
      date = this.monthsAgo(21);
    } else if (type === 'last-import-date') {
      date = this.props.lastImportDate;
    }

    this.props.onUserInput(date);
  }

  render() {
    return (
      <div className="btn-group" role="group">
        <a href="#" className="btn btn-default btn-lg dropdown-toggle" data-toggle="dropdown" role="button" aria-haspopup="true" aria-expanded="false"> {this.props.title} <span className="caret" /> </a>
        <ul className="dropdown-menu">
          <li><a href="#" onClick={this.handleDateChanged} type='now'>Now</a></li>
          <li><a href="#" onClick={this.handleDateChanged} type='one-day-ago'>Yesterday</a></li>
          <li><a href="#" onClick={this.handleDateChanged} type='two-days-ago'>2 days ago</a></li>
          <li><a href="#" onClick={this.handleDateChanged} type='one-week-ago'>1 week ago</a></li>
          <li><a href="#" onClick={this.handleDateChanged} type='two-weeks-ago'>2 weeks ago</a></li>
          <li><a href="#" onClick={this.handleDateChanged} type='three-weeks-ago'>3 weeks ago</a></li>
          <li><a href="#" onClick={this.handleDateChanged} type='one-month-ago'>1 month ago</a></li>
          {this.props.lastImportDate &&
              <span>
                <li role="separator" className="divider" />
                <li><a href="#" onClick={this.handleDateChanged} type='last-import-date'>Last Import</a></li>
              </span>
          }
          <li role="separator" className="divider" />
          <li><a href="#" onClick={this.handleDateChanged} type='reset'>Reset</a></li>
        </ul>
      </div>
    );
  }
}

export default DateDropdown;
