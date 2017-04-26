import React, { Component } from 'react';

class DateDropdown extends Component {
  hoursAgo = (hoursAgo) => { 
    var date = new Date();
    date.setHours(date.getHours() - hoursAgo);
    return date;
  }

  daysAgo = (daysAgo) => { 
    var date = new Date();
    date.setDate(date.getDate() - daysAgo);
    return date;
  }

  monthsAgo = (monthsAgo) => { 
    let date = new Date();
    date.setMonth(date.getMonth() - monthsAgo);
    return date;
  }

  handleDateChanged = (event) => {
    event.preventDefault();

    // Adjust the date for the user's selection.
    let date;
    let type = event.target.type;
    if (type === 'now') {
      date = new Date();
    } else if (type === 'one-hour-ago') {
      date = this.hoursAgo(1);
    } else if (type === 'twelve-hours-ago') {
      date = this.hoursAgo(12);
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
      date = this.monthsAgo(1);
    } else if (type === 'two-months-ago') {
      date = this.monthsAgo(2);
    } else if (type === 'three-months-ago') {
      date = this.monthsAgo(3);
    } else if (type === 'last-import-date') {
      date = this.props.lastImportDate;
    }

    this.props.onUserInput(date);
  }

  option = (text, type) => <li><a href="#" onClick={this.handleDateChanged} type={type}>{text}</a></li>;

  render() {
    return (
      <div className="btn-group">
        <a href="#" className="btn btn-default btn-lg dropdown-toggle" data-toggle="dropdown"> {this.props.title} <span className="caret" /> </a>
        <ul className="dropdown-menu">
          {this.option('Now', 'now')}
          <li role="separator" className="divider" />
          {this.option('1 hour ago', 'one-hour-ago')}
          {this.option('12 hours ago', 'twelve-hours-ago')}
          <li role="separator" className="divider" />
          {this.option('Yesterday', 'one-day-ago')}
          {this.option('2 days ago', 'two-days-ago')}
          {this.option('1 week ago', 'one-week-ago')}
          {this.option('2 weeks ago', 'two-weeks-ago')}
          {this.option('3 weeks ago', 'three-weeks-ago')}
          <li role="separator" className="divider" />
          {this.option('1 month ago', 'one-month-ago')}
          {this.option('2 months ago', 'two-months-ago')}
          {this.option('3 months ago', 'three-months-ago')}
          {this.props.lastImportDate &&
            <span>
              <li role="separator" className="divider" />
              {this.option('Last Import', 'last-import-date')}
            </span>
          }
          <li role="separator" className="divider" />
          {this.option('Reset', 'reset')}
        </ul>
      </div>
    );
  }
}

export default DateDropdown;
