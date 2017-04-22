import React, { Component } from 'react';
import { formatDifference, showDate } from '../../Common/Utilities' 
import PagedDataTableBar from '../Common/Data/PagedDataTableBar';
import DataTable from '../Common/Data/DataTable';
import ErrorPanel from '../Common/ErrorPanel';
import LoadingIndicator from '../Common/LoadingIndicator';
import Panel from '../Common/Panel';

class PostScrapeHistory extends Component {
  table = (scrapes) => {
    const mapping = [
      { name: 'Date',     key: scrape => showDate(scrape.importStart)                                     },
      { name: 'Since',    key: scrape => showDate(scrape.since)                                           },
      { name: 'Until',    key: scrape => showDate(scrape.until)                                           },
      { name: 'Pages',    key: scrape => scrape.pages.length                                              },
      { name: 'Posts',    key: scrape => scrape.numberOfPosts                                             },
      { name: 'Comments', key: scrape => scrape.numberOfComments                                          },
      { name: 'Took',     key: scrape => formatDifference(scrape.importStart, scrape.importEnd) + ' mins' }
    ];

    return (
      <Panel showHeading={false} table={true}>
        <DataTable startIndex={scrapes.startItemIndex + 1} minSize={12}
                   mapping={mapping} data={scrapes.data}
                   onRowSelected={this.props.onScrapeSelected}/>
      </Panel>
    );
  }

  render() {
    const { scrapes, errorMessage, onScrapesPaginationChanged } = this.props;
    if (errorMessage) {
      return <ErrorPanel message={errorMessage} />;
    } else if (!scrapes.data) {
      return <LoadingIndicator />
    }

    return (
      <div>
        <PagedDataTableBar {...scrapes}
            onPageSizeChanged={pageSize => onScrapesPaginationChanged(null, pageSize)}
            onPageNumberChanged={pageNumber => onScrapesPaginationChanged(pageNumber, null)} />
        {this.table(scrapes)}
      </div>
    );
  }
}

export default PostScrapeHistory;
