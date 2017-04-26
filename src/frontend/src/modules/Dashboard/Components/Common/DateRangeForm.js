import React, { Component } from 'react';
import moment from 'moment'
import DateDropdown from './DateDropdown';
import Modal from './Modal';

class DateRangeForm extends Component {
  constructor(props) {
    super(props);
    this.state = {since: props.since || '', until: props.until || ''};
  }

  handleSinceChange = (event) => this.setState({since: moment(event.target.value).toDate(), began: true});
  handleUntilChange = (event) => this.setState({until: moment(event.target.value).toDate(), began: true});

  updateSince = (since) => this.setState({since, began: true});
  updateUntil = (until) => this.setState({until, began: true});

  handleSubmit = (event, action) => {
    event.preventDefault();

    const { since, until } = this.state;
    let errorMessage = [];
    if (since && until) {
      if (since > until) {
        errorMessage.push(<p key='greater'>Since cannot be greater than until.</p>);
      }
      if (since > new Date()) {
        errorMessage.push(<p key='since-greater-than-now'>Since cannot refer to a point in the future.</p>);
      }
      if (until > new Date()) {
        errorMessage.push(<p key='until-greater-than-now'>Until cannot refer to a point in the future.</p>);
      }
    } else if (!this.props.allowEmpty) {
      if (!since) {
        errorMessage.push(<p key='no-since'>Since has no value.</p>);
      }

      if (!until) {
        errorMessage.push(<p key='no-until'>Until has no value.</p>);
      }
    }

    if (errorMessage.length === 0) {
      action(since || "Invalid", until || "Invalid");
      return true;
    }

    this.setState({errorMessage, began: true});
    window.showModal('#date-form-modal');
  }

  sinceIsValid = () => {
    if (!this.state.began) {
      return true;
    }
    if (!this.state.since) {
      return this.props.allowEmpty;
    }

    return (!this.state.until || this.state.since < this.state.until) && this.state.since <= new Date();
  }

  untilIsValid = () => {
    if (!this.state.began) {
      return true;
    }
    if (!this.state.until) {
      return this.props.allowEmpty;
    }

    return (!this.state.since || this.state.since < this.state.until) && this.state.until <= new Date();
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
      <form className="btn-toolbar" onSubmit={e => this.handleSubmit(e, this.props.onSubmit)}>
        <div className="btn-group">
          <DateDropdown title={this.props.lowerName} onUserInput={this.updateSince} />
          <DateDropdown title={this.props.upperName} onUserInput={this.updateUntil} />
        </div>
        <div className="btn-group">
          <input type="submit" className="btn btn-primary btn-lg" value={this.props.action} />
        </div>
        <div className="pull-right flex">
          <div className="form-inline">
            <div className={'form-group ' + (this.sinceIsValid() ? '' : 'has-error')}>
              <input value={this.formatDate(this.state.since)} onChange={this.handleSinceChange} className="form-control" type="datetime-local" name="since" />
            </div>
            <div className={'form-group ' + (this.untilIsValid() ? '' : 'has-error')}>
              <input value={this.formatDate(this.state.until)} onChange={this.handleUntilChange} className="form-control" type="datetime-local" name="until" />
            </div>
          </div>
          {this.props.extraButtonAction &&
            <div className="btn-group">
              <input type="button" className="btn btn-default btn-lg" value={this.props.extraButtonAction}
                     onClick={e => this.handleSubmit(e, this.props.onExtraButtonClicked)} />
            </div>          
          }
        </div>
        <Modal id="date-form-modal" title="Cannot scrape">{this.state.errorMessage}</Modal>
      </form>
    );
  }
}

export default DateRangeForm;
