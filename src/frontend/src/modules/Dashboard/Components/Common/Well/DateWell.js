import React, { Component } from 'react';
import Moment from 'react-moment';

class DateWell extends Component {
  static get defaultProps() { return {format: 'YYYY-MM-DD HH:mm'}; }

  render() {
    // Show a title if we have a date.
    let title;
    if (this.props.date || !this.props.fallback) {
      title = <h1><small className="text-muted">{this.props.title}</small></h1>;
    }

    // The user can provide a fallback in case the date is invalid or empty.
    let date;
    if (this.props.date) {
      date = <Moment format={this.props.format}>{this.props.date}</Moment>;  
    } else {
      date = this.props.fallback;
    }

    return (
      <div className="well">
        {title}
        <h1>{date}</h1>
      </div>
    );
  }
}

export default DateWell;
