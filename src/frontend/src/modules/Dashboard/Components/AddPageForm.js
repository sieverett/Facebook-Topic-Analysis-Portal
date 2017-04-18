import React, { Component } from 'react';
import Modal from './Common/Modal';
import Panel from './Common/Panel';
import SubmitButton from './Common/SubmitButton';

class AddPageForm extends Component {
  constructor(props) {
    super(props);

    // The user can set default values for the inputs to display.
    this.state = {
      name: props.name || '',
      facebookId: props.facebookId || '',
      modalId: 'add-page-modal'
    };
  }

  componentWillReceiveProps(nextProps) {
    // The user can clear the values of the inputs to display.
    if (nextProps.clear) {
      this.setState({name: '', facebookId: '', began: false});
      nextProps.onClear();
    }
  }

  handleNameChange= (event) => this.setState({name: event.target.value, began: true});

  handleFacebookIdChange = (event) => this.setState({facebookId: event.target.value, began: true});

  handleSubmit = (event) => {
    event.preventDefault();

    var errorMessage = [];
    if (!this.state.name) {
      errorMessage.push(<p key='name-empty'>Name must be non-empty.</p>);
    }
    if (!this.state.facebookId) {
      errorMessage.push(<p key='facebookId-empty'>Facebook ID must be non-empty.</p>);
    }

    if (errorMessage.length === 0) {
      this.props.onSubmit(this.state.name, this.state.facebookId);
      return true;
    }

    this.setState({'errorMessage': errorMessage, began: true});
    window.showModal('#' + this.state.modalId);
  }

  isNameValid = () => !this.state.began || this.state.name;
  isFacebookIdValid = () => !this.state.began || this.state.facebookId;

  render() {
    return (
      <Panel title={this.props.title}>
        <form onSubmit={this.handleSubmit}>
          <div className={'form-group ' + (!this.state.began || this.isNameValid() ? '' : 'has-error')}>
            <input className="form-control" placeholder="Name" value={this.state.name} onChange={this.handleNameChange} />
          </div>
          <div className='form-group'>
            <div className={'input-group ' + (!this.state.began || this.isFacebookIdValid() ? '' : 'has-error')}>
              <span className="input-group-addon" id="basic-addon3">https://facebook.com/</span>
              <input className="form-control" value={this.state.facebookId} onChange={this.handleFacebookIdChange} />
            </div>
          </div>
          <SubmitButton title={this.props.submitButtonTitle} />
        </form>
        <Modal id={this.state.modalId} title="Cannot add page">{this.state.errorMessage}</Modal>
      </Panel>
    );
  }
}

export default AddPageForm;
