import React, { Component } from 'react';
import Modal from '../Common/Modal';

class NumericValueForm extends Component {
  constructor(props) {
    super(props);
    this.state = {
      value: props.value,
      modalId: this.props.name.replace(' ', '') + '-modal'
    };
  }

  componentWillReceiveProps(nextProps) {
    // The user can directly set the value of the input to display.
    this.setState({'value': nextProps.value, began: true});
  }

  handleChange = (event) => this.setState({value: event.target.value, began: true});

  handleSubmit = (event) => {
    event.preventDefault();

    var errorMessage = [];
    const pageSize = parseInt(this.state.value, 10);
    if (isNaN(pageSize)) {
      errorMessage.push(<p key='not-int'>{this.props.name} must be an integer.</p>);
    } else if (this.props.minValue && pageSize < this.props.minValue) {
      errorMessage.push(<p key='too-small'>{this.props.name} must be greater than or equal to {this.props.minValue}.</p>);
    } else if (this.props.maxValue && pageSize > this.props.maxValue) {
      errorMessage.push(<p key='too-big'>{this.props.name} must be less than or equal to {this.props.maxValue}.</p>);
    }
    
    if (errorMessage.length === 0) {
      if (this.props.onSubmit) {
        this.props.onSubmit(this.state.value);
      }
      return true;
    }

    this.setState({'errorMessage': errorMessage, began: true});
    window.showModal('#' + this.state.modalId);
  }

  isValid = () => {
    if (!this.state.began) {
      return true;
    }

    return !isNaN(parseInt(this.state.value, 10)) &&
           (!this.props.minValue || this.state.value >= this.props.minValue) &&
           (!this.props.maxValue || this.state.value <= this.props.maxValue);
  }

  render() {
    return (
      <form onSubmit={this.handleSubmit} className="form-inline paged-form">
        <div className={'input-group ' + (this.isValid() ? '' : 'has-error')}>
          <input type="number" className="form-control" placeholder={this.props.placeholder} value={this.state.value} onChange={this.handleChange} />
          <span className="input-group-btn" role="group">
            <input type="submit" className="btn btn-primary" value={this.props.action}/>
          </span>
        </div>
        <Modal id={this.state.modalId} title="Cannot scrape">{this.state.errorMessage}</Modal>
      </form>
    );
  }
}

export default NumericValueForm;
