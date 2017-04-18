import React, { Component } from 'react';
import moment from 'moment'
import DateDropdown from './DateDropdown';
import Modal from './Modal';

class DateRangeForm extends Component {
  state = {since: '', until: ''}

  handleSinceChange = (event) => this.setState({since: moment(event.target.value).toDate(), began: true});
  handleUntilChange = (event) => this.setState({until: moment(event.target.value).toDate(), began: true});

  updateSince = (val) => this.setState({since: val, began: true});
  updateUntil = (val) => this.setState({until: val, began: true});

  handleSubmit = (event) => {
    event.preventDefault();

    var errorMessage = [];
    if (this.state.since && this.state.until) {
      if (this.state.since > this.state.until) {
        errorMessage.push(<p key='greater'>Since cannot be greater than until.</p>);
      }
      if (this.state.since > new Date()) {
        errorMessage.push(<p key='since-greater-than-now'>Since cannot refer to a point in the future.</p>);
      }
      if (this.state.until > new Date()) {
        errorMessage.push(<p key='until-greater-than-now'>Until cannot refer to a point in the future.</p>);
      }
    } else {
      if (!this.state.since) {
        errorMessage.push(<p key='no-since'>Since has no value.</p>);
      }

      if (!this.state.until) {
        errorMessage.push(<p key='no-until'>Until has no value.</p>);
      }
    }

    if (errorMessage.length === 0) {
      this.props.onSubmit(this.state.since, this.state.until);
      return true;
    }

    this.setState({'errorMessage': errorMessage, began: true});
    window.showModal('#date-form-modal');
  }

  sinceIsValid = () => {
    if (!this.state.began) {
      return true;
    }

    return this.state.since && (!this.state.until || this.state.since < this.state.until) && this.state.since <= new Date();
  }

  untilIsValid = () => {
    if (!this.state.began) {
      return true;
    }

    return this.state.until && (!this.state.since || this.state.since < this.state.until) && this.state.until <= new Date();
  }

  formatDate = (date) => {
    // Parse the date into a human readable format that <input type="datetime-local" /> understands.
    if (!date) {
      return '';
    }

    return moment(date).format('YYYY-MM-DDTHH:mm');
  }

  render() {
    return (
      <div className="row sub-header">
        <form onSubmit={this.handleSubmit}>
          <div className="btn-group btn-group-justified" role="group" aria-label="Justified button group with nested dropdown">
            <DateDropdown title="Since" onUserInput={this.updateSince} />
            <DateDropdown title="Until" onUserInput={this.updateUntil} />
            <div className="btn-group" role="group">
              <input type="submit" className="btn btn-primary btn-lg" value={this.props.action} />
            </div>
          </div>

          <div className="form-inline" style={{"marginTop": "20px", "marginBottom": "10px"}}>
            <div className={'form-group ' + (this.sinceIsValid() ? '' : 'has-error')}>
              <label htmlFor="since-input" style={{"margin": "0 10px"}}>Since</label>
              <input value={this.formatDate(this.state.since)} onChange={this.handleSinceChange} className="form-control" type="datetime-local" name="since" />
            </div>
            <div className={'form-group ' + (this.untilIsValid() ? '' : 'has-error')}>
              <label htmlFor="until-input" style={{"marginLeft": "10px", "marginRight": "10px"}}>Until</label>
              <input value={this.formatDate(this.state.until)} onChange={this.handleUntilChange} className="form-control" type="datetime-local" name="until" />
            </div>
          </div>
          <Modal id="date-form-modal" title="Cannot scrape">{this.state.errorMessage}</Modal>
        </form>
      </div>
    );
  }
}

export default DateRangeForm;
